namespace Ieh.OwneyBeauty.Modules.Accounts;

public static class AccountsSettingNames
{
    private const string Prefix = "Owney.Accounts";

    public const string VerificationCodeLifetimeMinutes = Prefix + ".Verification.CodeLifetimeMinutes";
    public const string VerificationMaxAttempts = Prefix + ".Verification.MaxAttempts";
    public const string VerificationResendIntervalSeconds = Prefix + ".Verification.ResendIntervalSeconds";
    public const string VerificationDailyLimit = Prefix + ".Verification.DailyLimit";
}
