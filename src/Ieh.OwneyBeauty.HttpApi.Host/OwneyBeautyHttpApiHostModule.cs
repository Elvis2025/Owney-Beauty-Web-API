using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server.AspNetCore;
using Ieh.OwneyBeauty.EntityFrameworkCore;
using Ieh.OwneyBeauty.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using Ieh.OwneyBeauty.MultiTenancy;
using Ieh.OwneyBeauty.HealthChecks;
using Microsoft.OpenApi;
using Volo.Abp;
using Volo.Abp.Studio;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Microsoft.AspNetCore.Hosting;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Security.Claims;

namespace Ieh.OwneyBeauty;

[DependsOn(
    typeof(OwneyBeautyHttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(OwneyBeautyApplicationModule),
    typeof(OwneyBeautyEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(OwneyBeautyInfrastructureModule)
    )]
public class OwneyBeautyHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("OwneyBeauty");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        // ACC-F05 / OB-47-003/004: access token corto y refresh token rotativo (OpenIddict rota en cada uso).
        PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
        {
            serverBuilder.SetAccessTokenLifetime(TimeSpan.FromMinutes(configuration.GetValue("AuthServer:AccessTokenLifetimeMinutes", 15)));
            serverBuilder.SetRefreshTokenLifetime(TimeSpan.FromDays(configuration.GetValue("AuthServer:RefreshTokenLifetimeDays", 14)));
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                var passPhrase = configuration["AuthServer:CertificatePassPhrase"]!;
                var certificateBase64 = configuration["AuthServer:CertificateBase64"];
                if (!string.IsNullOrWhiteSpace(certificateBase64))
                {
                    // Railway/contenedores: el .pfx llega como variable de entorno en base64 (DEV-F03).
                    var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(certificateBase64), passPhrase, X509KeyStorageFlags.EphemeralKeySet);
                    serverBuilder.AddSigningCertificate(certificate);
                    serverBuilder.AddEncryptionCertificate(certificate);
                }
                else
                {
                    serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", passPhrase);
                }

                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
            
            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        if (hostingEnvironment.IsDevelopment())
        {
            context.Services.AddRazorPages()
                .AddRazorRuntimeCompilation();
        }

        ConfigureStudio(hostingEnvironment);
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles(hostingEnvironment);
        ConfigureConventionalControllers();
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureDataProtection(context, configuration);
        context.Services.AddOwneyRateLimiting(configuration);
        context.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
        });
    }

    private static void ConfigureDataProtection(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var dataProtection = context.Services.AddDataProtection().SetApplicationName("OwneyBeauty");
        if (OwneyBeautyInfrastructureModule.IsRedisEnabled(configuration))
        {
            // Las claves sobreviven a reinicios y se comparten entre réplicas (PLT-F10).
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtection.PersistKeysToStackExchangeRedis(redis, "Owney:DataProtection-Keys");
        }
    }

    private void ConfigureStudio(IHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsProduction())
        {
            Configure<AbpStudioClientOptions>(options =>
            {
                options.IsLinkEnabled = false;
            });
        }
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:AngularUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles(IHostEnvironment hostingEnvironment)
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                    if (hostingEnvironment.IsDevelopment())
                    {
                        bundle.AddFiles("/dev-login-helper.js");
                    }
                }
            );
        });
    }


    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<OwneyBeautyDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Ieh.OwneyBeauty.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<OwneyBeautyDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Ieh.OwneyBeauty.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<OwneyBeautyApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Ieh.OwneyBeauty.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<OwneyBeautyApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Ieh.OwneyBeauty.Application"));
            });
        }
    }

    /// <summary>Módulos de negocio (carpetas Modules/&lt;Módulo&gt; en cada capa).</summary>
    private static readonly string[] OwneyModules = { "Platform", "Accounts" };

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            var application = typeof(OwneyBeautyApplicationModule).Assembly;

            // Cada carpeta Modules/<Módulo> expone su API bajo su propio prefijo y nombre de servicio remoto
            // (/api/platform/..., /api/accounts/...; lo usan los proxies de `abp generate-proxy -m <Módulo>`).
            foreach (var module in OwneyModules)
            {
                var ns = $"Ieh.OwneyBeauty.Modules.{module}.";
                options.ConventionalControllers.Create(application, opts =>
                {
                    opts.RootPath = module.ToLowerInvariant();
                    opts.RemoteServiceName = module;
                    opts.TypePredicate = type => type.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true;
                });
            }

            // Servicios del host que no pertenecen a un módulo (/api/app/...).
            options.ConventionalControllers.Create(application, opts =>
            {
                opts.TypePredicate = type => type.Namespace?.StartsWith("Ieh.OwneyBeauty.Modules.", StringComparison.Ordinal) != true;
            });
        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOidc(
            configuration["AuthServer:Authority"]!,
            ["OwneyBeauty"],
            [AbpSwaggerOidcFlows.AuthorizationCode],
            null,
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "OwneyBeauty API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddOwneyBeautyHealthChecks();
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();
        app.UseCorrelationId();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseRateLimiter();
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseMiddleware<IdempotencyMiddleware>();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OwneyBeauty API");
            options.DocumentTitle = "Owney Beauty API";
            options.InjectStylesheet("/swagger-owney.css");
            options.HeadContent += "<link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico\" />";

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
