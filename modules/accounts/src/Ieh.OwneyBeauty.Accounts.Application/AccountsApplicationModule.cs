using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Ieh.OwneyBeauty.Accounts.Legal;
using Ieh.OwneyBeauty.Accounts.Localization;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Application;
using Volo.Abp.Application.Services;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Accounts;

[DependsOn(
    typeof(AccountsDomainModule),
    typeof(AccountsApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class AccountsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AccountsApplicationModule>();
    }
}

public abstract class AccountsAppService : ApplicationService
{
    protected AccountsAppService()
    {
        LocalizationResource = typeof(AccountsResource);
        ObjectMapperContext = typeof(AccountsApplicationModule);
    }
}

/// <summary>
/// Correo interno para cuentas registradas solo con teléfono. ABP Identity exige un correo;
/// el dominio ".invalid" está reservado (RFC 2606) y nunca entrega mensajes. Nunca se expone al cliente.
/// </summary>
public static class PlaceholderEmail
{
    public const string Domain = "phone.owneybeauty.invalid";

    public static string For(Guid userId) => $"{userId:N}@{Domain}";

    public static bool Is(string? email) =>
        email is not null && email.EndsWith("@" + Domain, StringComparison.OrdinalIgnoreCase);

    public static string? Public(string? email) => Is(email) ? null : email;
}

public static class ContactMasking
{
    public static string Mask(string value)
    {
        var at = value.IndexOf('@');
        if (at > 0)
        {
            var visible = Math.Min(2, at);
            return value[..visible] + new string('*', Math.Max(3, at - visible)) + value[at..];
        }

        return value.Length <= 6 ? new string('*', value.Length) : value[..5] + new string('*', value.Length - 7) + value[^2..];
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LegalDocumentToLegalDocumentDtoMapper : MapperBase<LegalDocument, LegalDocumentDto>
{
    public override partial LegalDocumentDto Map(LegalDocument source);
    public override partial void Map(LegalDocument source, LegalDocumentDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LegalDocumentToPublicLegalDocumentDtoMapper : MapperBase<LegalDocument, PublicLegalDocumentDto>
{
    public override partial PublicLegalDocumentDto Map(LegalDocument source);
    public override partial void Map(LegalDocument source, PublicLegalDocumentDto destination);
}
