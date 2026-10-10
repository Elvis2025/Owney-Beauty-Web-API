namespace Ieh.OwneyBeauty.Modules.Accounts;

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
