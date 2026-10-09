using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Accounts;

public static class AccountsSettingNames
{
    private const string Prefix = "Owney.Accounts";

    public const string VerificationCodeLifetimeMinutes = Prefix + ".Verification.CodeLifetimeMinutes";
    public const string VerificationMaxAttempts = Prefix + ".Verification.MaxAttempts";
    public const string VerificationResendIntervalSeconds = Prefix + ".Verification.ResendIntervalSeconds";
    public const string VerificationDailyLimit = Prefix + ".Verification.DailyLimit";
}

public class AccountsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(AccountsSettingNames.VerificationCodeLifetimeMinutes, "10"),
            new SettingDefinition(AccountsSettingNames.VerificationMaxAttempts, "5"),
            new SettingDefinition(AccountsSettingNames.VerificationResendIntervalSeconds, "60"),
            new SettingDefinition(AccountsSettingNames.VerificationDailyLimit, "10")
        );
    }
}
