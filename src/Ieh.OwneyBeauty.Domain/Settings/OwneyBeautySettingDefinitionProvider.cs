using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Settings;

public class OwneyBeautySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(OwneyBeautySettings.MySetting1));
    }
}
