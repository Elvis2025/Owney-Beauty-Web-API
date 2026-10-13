using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.Uow;

namespace Ieh.OwneyBeauty.OpenIddict;

/* Creates initial data that is needed to property run the application
 * and make client-to-server communication possible.
 */
public class OpenIddictDataSeedContributor : OpenIddictDataSeedContributorBase, IDataSeedContributor, ITransientDependency
{
    public OpenIddictDataSeedContributor(
        IConfiguration configuration,
        IOpenIddictApplicationRepository openIddictApplicationRepository,
        IAbpApplicationManager applicationManager,
        IOpenIddictScopeRepository openIddictScopeRepository,
        IOpenIddictScopeManager scopeManager)
        : base(configuration, openIddictApplicationRepository, applicationManager, openIddictScopeRepository, scopeManager)
    {
        _applicationManager = applicationManager;
    }

    private readonly IAbpApplicationManager _applicationManager;

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }

    /// <summary>Exige PKCE al cliente público (un código de autorización robado no sirve sin el verificador).</summary>
    private async Task RequirePkceAsync(string clientId)
    {
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            return;
        }

        var descriptor = new AbpApplicationDescriptor();
        await _applicationManager.PopulateAsync(descriptor, application);
        if (descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange))
        {
            await _applicationManager.UpdateAsync(application, descriptor);
        }
    }

    private async Task CreateScopesAsync()
    {
        await CreateScopesAsync(new OpenIddictScopeDescriptor 
        {
            Name = "OwneyBeauty", 
            DisplayName = "OwneyBeauty API", 
            Resources = { "OwneyBeauty" }
        });
    }

    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string> {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess,
            "OwneyBeauty"
        };

        var configurationSection = Configuration.GetSection("OpenIddict:Applications");


        // Cliente público de Owney (web Angular y apps móviles) — ACC-F04.
        // Flujo recomendado: Authorization Code + PKCE (obligatorio). El password grant queda solo
        // como transición para el login actual y se desactiva con OpenIddict:Applications:OwneyBeauty_App:AllowPasswordFlow=false.
        var appClientId = configurationSection["OwneyBeauty_App:ClientId"];
        if (!appClientId.IsNullOrWhiteSpace())
        {
            var appClientRootUrl = configurationSection["OwneyBeauty_App:RootUrl"]?.TrimEnd('/');
            var extraRedirects = (configurationSection["OwneyBeauty_App:ExtraRedirectUris"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var redirectUris = new List<string> { appClientRootUrl! };
            redirectUris.AddRange(extraRedirects);

            var grantTypes = new List<string>
            {
                OpenIddictConstants.GrantTypes.AuthorizationCode,
                OpenIddictConstants.GrantTypes.RefreshToken,
            };

            if (bool.TryParse(configurationSection["OwneyBeauty_App:AllowPasswordFlow"], out var allowPassword) && allowPassword)
            {
                grantTypes.Add(OpenIddictConstants.GrantTypes.Password);
            }

            await CreateOrUpdateApplicationAsync(
                applicationType: OpenIddictConstants.ApplicationTypes.Web,
                name: appClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Owney Beauty App",
                secret: null,
                grantTypes: grantTypes,
                scopes: commonScopes,
                redirectUris: redirectUris,
                postLogoutRedirectUris: redirectUris,
                clientUri: appClientRootUrl,
                logoUri: "/images/logo/owney-badge-512.png"
            );

            await RequirePkceAsync(appClientId!);
        }

        // Swagger Client
        var swaggerClientId = configurationSection["OwneyBeauty_Swagger:ClientId"];
        if (!swaggerClientId.IsNullOrWhiteSpace())
        {
            var swaggerRootUrl = configurationSection["OwneyBeauty_Swagger:RootUrl"]?.TrimEnd('/');

            await CreateOrUpdateApplicationAsync(
                applicationType: OpenIddictConstants.ApplicationTypes.Web,
                name: swaggerClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Swagger Application",
                secret: null,
                grantTypes: new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode, },
                scopes: commonScopes,
                redirectUris: new List<string> { $"{swaggerRootUrl}/swagger/oauth2-redirect.html" },
                clientUri: swaggerRootUrl.EnsureEndsWith('/') + "swagger",
                logoUri: "/images/clients/swagger.svg"
            );
        }


    }
}
