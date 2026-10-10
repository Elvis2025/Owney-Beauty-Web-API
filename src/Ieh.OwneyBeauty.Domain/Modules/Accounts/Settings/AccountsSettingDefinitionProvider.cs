using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Modules.Accounts.Settings;

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
