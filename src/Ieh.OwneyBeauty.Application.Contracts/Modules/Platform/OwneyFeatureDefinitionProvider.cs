using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;
using Ieh.OwneyBeauty.Localization;

namespace Ieh.OwneyBeauty.Modules.Platform;

public class OwneyFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(OwneyFeatures.GroupName, L("Feature:Owney"));
        Add(group, OwneyFeatures.Social);
        Add(group, OwneyFeatures.Bookings);
        Add(group, OwneyFeatures.HomeService);
        Add(group, OwneyFeatures.Payments);
        Add(group, OwneyFeatures.Messaging);
    }

    private static void Add(FeatureGroupDefinition group, string name) =>
        group.AddFeature(name, defaultValue: "true", displayName: L("Feature:" + name), valueType: new ToggleStringValueType(), isVisibleToClients: true);

    private static LocalizableString L(string name) => LocalizableString.Create<OwneyBeautyResource>(name);
}
