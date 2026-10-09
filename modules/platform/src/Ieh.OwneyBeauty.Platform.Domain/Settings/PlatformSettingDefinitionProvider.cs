using Ieh.OwneyBeauty.Platform.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Platform.Settings;

public class PlatformSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(PlatformSettingNames.Market, PlatformConsts.DefaultMarket, L("Setting:" + PlatformSettingNames.Market), isVisibleToClients: true),
            new SettingDefinition(PlatformSettingNames.DefaultCurrency, SharedKernel.Money.DefaultCurrency, L("Setting:" + PlatformSettingNames.DefaultCurrency), isVisibleToClients: true),
            new SettingDefinition(PlatformSettingNames.DefaultTimeZone, PlatformConsts.DefaultTimeZone, L("Setting:" + PlatformSettingNames.DefaultTimeZone), isVisibleToClients: true),
            new SettingDefinition(PlatformSettingNames.IdempotencyRetentionHours, "24", L("Setting:" + PlatformSettingNames.IdempotencyRetentionHours))
        );
    }

    private static LocalizableString L(string name) => LocalizableString.Create<PlatformResource>(name);
}
