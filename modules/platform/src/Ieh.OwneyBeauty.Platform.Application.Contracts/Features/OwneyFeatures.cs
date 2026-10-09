using Ieh.OwneyBeauty.Platform.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Ieh.OwneyBeauty.Platform.Features;

/// <summary>
/// Interruptores por módulo (PLT-M05). Permiten activar funciones de forma gradual y apagarlas
/// en caso de incidente sin redesplegar (kill-switch, §16.2 del documento maestro).
/// </summary>
public static class OwneyFeatures
{
    public const string GroupName = "Owney";

    public const string Social = GroupName + ".Social";
    public const string Bookings = GroupName + ".Bookings";
    public const string HomeService = GroupName + ".HomeService";
    public const string Payments = GroupName + ".Payments";
    public const string Messaging = GroupName + ".Messaging";
}

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

    private static LocalizableString L(string name) => LocalizableString.Create<PlatformResource>(name);
}
