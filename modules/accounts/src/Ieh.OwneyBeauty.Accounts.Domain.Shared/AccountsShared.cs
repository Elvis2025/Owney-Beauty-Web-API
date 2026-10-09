using System;
using Ieh.OwneyBeauty.SharedKernel;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;

namespace Ieh.OwneyBeauty.Accounts.Localization
{
    [LocalizationResourceName("Accounts")]
    public class AccountsResource
    {
    }
}

namespace Ieh.OwneyBeauty.Accounts
{
    public static class AccountsDbProperties
    {
        public const string DbSchema = "accounts";
        public const string DbTablePrefix = "";
        public const string ConnectionStringName = "Accounts";
    }

    public static class AccountsErrorCodes
    {
        public const string Namespace = OwneyErrorCodes.Prefix + ".Accounts";

        public const string RegistrationFailed = Namespace + ":RegistrationFailed";
        public const string EmailOrPhoneRequired = Namespace + ":EmailOrPhoneRequired";
        public const string UserNameTaken = Namespace + ":UserNameTaken";
        public const string LegalDocumentsNotAccepted = Namespace + ":LegalDocumentsNotAccepted";
        public const string ModeNotAvailable = Namespace + ":ModeNotAvailable";
        public const string ModeNotSupportedYet = Namespace + ":ModeNotSupportedYet";
        public const string NoEmailToVerify = Namespace + ":NoEmailToVerify";
        public const string AlreadyVerified = Namespace + ":AlreadyVerified";
        public const string VerificationCodeInvalid = Namespace + ":VerificationCodeInvalid";
        public const string VerificationCodeExpired = Namespace + ":VerificationCodeExpired";
        public const string VerificationTooManyAttempts = Namespace + ":VerificationTooManyAttempts";
        public const string VerificationResendTooSoon = Namespace + ":VerificationResendTooSoon";
        public const string VerificationDailyLimitReached = Namespace + ":VerificationDailyLimitReached";
        public const string PhoneNumberInvalid = Namespace + ":PhoneNumberInvalid";
        public const string LegalDocumentAlreadyPublished = Namespace + ":LegalDocumentAlreadyPublished";
        public const string LegalDocumentNotPublished = Namespace + ":LegalDocumentNotPublished";
        public const string LegalDocumentVersionExists = Namespace + ":LegalDocumentVersionExists";
    }

    public static class AccountsConsts
    {
        public const int MaxUserNameLength = 64;
        public const int MaxNameLength = 64;
        public const int MaxEmailLength = 256;
        public const int MaxPhoneLength = 16;
        /// <summary>E.164: + seguido de 8 a 15 dígitos. RD: +1809/+1829/+1849.</summary>
        public const string PhoneRegex = @"^\+[1-9]\d{7,14}$";
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;

        public const int VerificationCodeLength = 6;
        public const int MaxVerificationTargetLength = 256;
        public const int VerificationCodeHashLength = 64;

        public const int MaxLegalVersionLength = 32;
        public const int MaxLegalTitleLength = 256;
        public const int MaxLegalUrlLength = 512;
        public const int MaxLegalContentLength = 200_000;
        public const int MaxLanguageLength = 8;
        public const int MaxClientIpLength = 64;
    }

    /// <summary>Modos de uso de una misma cuenta (OB-02-002/003).</summary>
    public enum AccountMode
    {
        Client = 1,
        Professional = 2,
        Business = 3,
    }

    public enum OnboardingStep
    {
        RoleSelection = 0,
        Profile = 1,
        Verification = 2,
        Completed = 3,
    }

    public enum VerificationChannel
    {
        Email = 1,
        Phone = 2,
    }

    public enum LegalDocumentType
    {
        TermsOfService = 1,
        PrivacyPolicy = 2,
        CancellationPolicy = 3,
        ProfessionalAgreement = 4,
        ContentPolicy = 5,
    }

    public enum ConsentType
    {
        DataProcessing = 1,
        Marketing = 2,
        Location = 3,
    }

    /// <summary>Roles del sistema (ACC-M01). "admin" es el rol que ABP crea por defecto.</summary>
    public static class OwneyRoles
    {
        public const string Admin = "admin";
        public const string Client = "client";
        public const string Professional = "professional";
        public const string BusinessOwner = "business-owner";
        public const string Employee = "employee";
        public const string KycAnalyst = "kyc-analyst";
        public const string Support = "support";
        public const string Moderator = "moderator";
        public const string Finance = "finance";
        public const string Marketing = "marketing";

        public static readonly string[] All =
        {
            Client, Professional, BusinessOwner, Employee, KycAnalyst, Support, Moderator, Finance, Marketing,
        };

        /// <summary>Roles internos de alto riesgo: MFA obligatorio antes del go-live (ACC-F10).</summary>
        public static readonly string[] Internal = { Admin, KycAnalyst, Support, Moderator, Finance, Marketing };
    }

    /// <summary>Evento de dominio "UserRegistered" (§14.3). Se publica por outbox.</summary>
    [EventName("Owney.Accounts.UserRegistered")]
    public class UserRegisteredEto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public AccountMode InitialMode { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    [EventName("Owney.Accounts.AccountModeChanged")]
    public class AccountModeChangedEto
    {
        public Guid UserId { get; set; }
        public AccountMode PreviousMode { get; set; }
        public AccountMode NewMode { get; set; }
    }

    [EventName("Owney.Accounts.ContactVerified")]
    public class ContactVerifiedEto
    {
        public Guid UserId { get; set; }
        public VerificationChannel Channel { get; set; }
    }
}
