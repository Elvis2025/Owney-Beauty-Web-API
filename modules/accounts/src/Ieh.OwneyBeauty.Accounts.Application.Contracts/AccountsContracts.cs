using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Accounts.Localization;
using Volo.Abp.Application;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Accounts
{
    [DependsOn(
        typeof(AccountsDomainSharedModule),
        typeof(AbpDddApplicationContractsModule),
        typeof(AbpAuthorizationAbstractionsModule)
    )]
    public class AccountsApplicationContractsModule : AbpModule
    {
    }
}

namespace Ieh.OwneyBeauty.Accounts.Permissions
{
    public static class AccountsPermissions
    {
        public const string GroupName = "Accounts";

        public static class LegalDocuments
        {
            public const string Default = GroupName + ".LegalDocuments";
            public const string Manage = Default + ".Manage";
        }
    }

    public class AccountsPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var group = context.AddGroup(AccountsPermissions.GroupName, L("Permission:Accounts"));
            group.AddPermission(AccountsPermissions.LegalDocuments.Default, L("Permission:Accounts.LegalDocuments"))
                .AddChild(AccountsPermissions.LegalDocuments.Manage, L("Permission:Accounts.LegalDocuments.Manage"));
        }

        private static LocalizableString L(string name) => LocalizableString.Create<AccountsResource>(name);
    }
}

namespace Ieh.OwneyBeauty.Accounts.Registration
{
    public class RegisterDto : IValidatableObject
    {
        [EmailAddress, StringLength(AccountsConsts.MaxEmailLength)]
        public string? Email { get; set; }

        /// <summary>Formato E.164, p. ej. +18095551234.</summary>
        [RegularExpression(AccountsConsts.PhoneRegex), StringLength(AccountsConsts.MaxPhoneLength)]
        public string? PhoneNumber { get; set; }

        /// <summary>Opcional. Si no se envía se genera a partir del correo/teléfono.</summary>
        [StringLength(AccountsConsts.MaxUserNameLength, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9._]+$")]
        public string? UserName { get; set; }

        [Required, StringLength(AccountsConsts.MaxNameLength)]
        public string Name { get; set; } = null!;

        [StringLength(AccountsConsts.MaxNameLength)]
        public string? Surname { get; set; }

        [Required, StringLength(AccountsConsts.MaxPasswordLength, MinimumLength = AccountsConsts.MinPasswordLength)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>Cliente o Profesional (Negocio llega en V2).</summary>
        public AccountMode InitialMode { get; set; } = AccountMode.Client;

        /// <summary>Ids de los documentos legales vigentes que el usuario acepta.</summary>
        public List<Guid> AcceptedLegalDocumentIds { get; set; } = new();

        public bool MarketingConsent { get; set; }

        /// <summary>Idioma preferido (es/en) para comunicaciones y documentos legales.</summary>
        [StringLength(AccountsConsts.MaxLanguageLength)]
        public string Language { get; set; } = "es";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("Email o teléfono requerido.", new[] { nameof(Email), nameof(PhoneNumber) });
            }
        }
    }

    public class RegistrationResultDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public AccountMode ActiveMode { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public bool RequiresPhoneVerification { get; set; }
    }

    public interface IRegistrationAppService : IApplicationService
    {
        /// <summary>Registro público (OB-01-001). Tras registrarse, el cliente inicia sesión por OAuth.</summary>
        Task<RegistrationResultDto> RegisterAsync(RegisterDto input);
    }
}

namespace Ieh.OwneyBeauty.Accounts.Verification
{
    public class SendPhoneCodeDto
    {
        /// <summary>Si se indica, reemplaza el teléfono actual (pendiente de verificación).</summary>
        [RegularExpression(AccountsConsts.PhoneRegex), StringLength(AccountsConsts.MaxPhoneLength)]
        public string? PhoneNumber { get; set; }
    }

    public class ConfirmCodeDto
    {
        [Required, StringLength(AccountsConsts.VerificationCodeLength, MinimumLength = AccountsConsts.VerificationCodeLength)]
        [RegularExpression(@"^\d+$")]
        public string Code { get; set; } = null!;
    }

    public class VerificationCodeSentDto
    {
        public VerificationChannel Channel { get; set; }

        /// <summary>Destino enmascarado, p. ej. "ma***@gmail.com" o "+1809*****34".</summary>
        public string MaskedTarget { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>Verificación de correo y teléfono del usuario autenticado (OB-01-002/003).</summary>
    public interface IVerificationAppService : IApplicationService
    {
        Task<VerificationCodeSentDto> SendEmailCodeAsync();
        Task ConfirmEmailAsync(ConfirmCodeDto input);
        Task<VerificationCodeSentDto> SendPhoneCodeAsync(SendPhoneCodeDto input);
        Task ConfirmPhoneAsync(ConfirmCodeDto input);
    }
}

namespace Ieh.OwneyBeauty.Accounts.Legal
{
    public class LegalDocumentDto : FullAuditedEntityDto<Guid>
    {
        public LegalDocumentType Type { get; set; }
        public string Language { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public string? ExternalUrl { get; set; }
        public bool RequiresAcceptance { get; set; }
        public DateTime? PublishedTime { get; set; }
        public bool IsCurrent { get; set; }
    }

    /// <summary>Versión pública reducida (sin datos de auditoría).</summary>
    public class PublicLegalDocumentDto : EntityDto<Guid>
    {
        public LegalDocumentType Type { get; set; }
        public string Language { get; set; } = null!;
        public string Version { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public string? ExternalUrl { get; set; }
        public bool RequiresAcceptance { get; set; }
        public DateTime? PublishedTime { get; set; }
    }

    public class CreateLegalDocumentDto
    {
        public LegalDocumentType Type { get; set; }

        [Required, StringLength(AccountsConsts.MaxLanguageLength)]
        public string Language { get; set; } = "es";

        [Required, StringLength(AccountsConsts.MaxLegalVersionLength)]
        public string Version { get; set; } = null!;

        [Required, StringLength(AccountsConsts.MaxLegalTitleLength)]
        public string Title { get; set; } = null!;

        [StringLength(AccountsConsts.MaxLegalContentLength)]
        public string? Content { get; set; }

        [Url, StringLength(AccountsConsts.MaxLegalUrlLength)]
        public string? ExternalUrl { get; set; }

        public bool RequiresAcceptance { get; set; } = true;
    }

    public class UpdateLegalDocumentDto
    {
        [Required, StringLength(AccountsConsts.MaxLegalTitleLength)]
        public string Title { get; set; } = null!;

        [StringLength(AccountsConsts.MaxLegalContentLength)]
        public string? Content { get; set; }

        [Url, StringLength(AccountsConsts.MaxLegalUrlLength)]
        public string? ExternalUrl { get; set; }

        public bool RequiresAcceptance { get; set; } = true;
    }

    public class GetLegalDocumentsInput : PagedAndSortedResultRequestDto
    {
        public LegalDocumentType? Type { get; set; }
        public string? Language { get; set; }
        public bool? IsCurrent { get; set; }
    }

    public interface ILegalDocumentAppService : IApplicationService
    {
        /// <summary>Público: documentos vigentes en un idioma (con respaldo en español).</summary>
        Task<ListResultDto<PublicLegalDocumentDto>> GetCurrentAsync(string language = "es");

        Task<PagedResultDto<LegalDocumentDto>> GetListAsync(GetLegalDocumentsInput input);
        Task<LegalDocumentDto> GetAsync(Guid id);
        Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentDto input);
        Task<LegalDocumentDto> UpdateAsync(Guid id, UpdateLegalDocumentDto input);
        Task<LegalDocumentDto> PublishAsync(Guid id);
    }
}

namespace Ieh.OwneyBeauty.Accounts.MyAccount
{
    public class MyAccountDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<AccountMode> AvailableModes { get; set; } = new();
        public AccountMode ActiveMode { get; set; }
        public OnboardingStep OnboardingStep { get; set; }
        public List<Legal.PublicLegalDocumentDto> PendingLegalDocuments { get; set; } = new();
    }

    public class SwitchModeDto
    {
        public AccountMode Mode { get; set; }
    }

    public class EnableModeDto
    {
        public AccountMode Mode { get; set; }
    }

    public class AdvanceOnboardingDto
    {
        public OnboardingStep Step { get; set; }
    }

    public class AcceptLegalDocumentsDto
    {
        [Required, MinLength(1)]
        public List<Guid> LegalDocumentIds { get; set; } = new();
    }

    public class ConsentStatusDto
    {
        public ConsentType Type { get; set; }
        public bool Granted { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateConsentDto
    {
        public ConsentType Type { get; set; }
        public bool Granted { get; set; }
    }

    /// <summary>Cuenta del usuario autenticado: modos, onboarding, documentos legales y consentimientos.</summary>
    public interface IMyAccountAppService : IApplicationService
    {
        Task<MyAccountDto> GetAsync();

        /// <summary>Cambia de modo sin cerrar sesión (OB-02-003). No concede permisos.</summary>
        Task<MyAccountDto> SwitchModeAsync(SwitchModeDto input);

        /// <summary>Habilita un modo adicional en la misma cuenta (cuenta híbrida, OB-02-002).</summary>
        Task<MyAccountDto> EnableModeAsync(EnableModeDto input);

        Task<MyAccountDto> AdvanceOnboardingAsync(AdvanceOnboardingDto input);

        Task<MyAccountDto> AcceptLegalDocumentsAsync(AcceptLegalDocumentsDto input);

        Task<ListResultDto<ConsentStatusDto>> GetConsentsAsync();

        Task<ConsentStatusDto> UpdateConsentAsync(UpdateConsentDto input);
    }
}
