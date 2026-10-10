using Ieh.OwneyBeauty.Common;
namespace Ieh.OwneyBeauty.Modules.Accounts;

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
