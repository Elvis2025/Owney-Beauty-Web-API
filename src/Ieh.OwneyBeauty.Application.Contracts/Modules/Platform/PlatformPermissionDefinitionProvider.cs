using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Ieh.OwneyBeauty.Localization;

namespace Ieh.OwneyBeauty.Modules.Platform;

public class PlatformPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(PlatformPermissions.GroupName, L("Permission:Platform"));

        group.AddPermission(PlatformPermissions.Geography.Default, L("Permission:Platform.Geography"))
            .AddChild(PlatformPermissions.Geography.Manage, L("Permission:Platform.Geography.Manage"));

        group.AddPermission(PlatformPermissions.Currencies.Default, L("Permission:Platform.Currencies"))
            .AddChild(PlatformPermissions.Currencies.Manage, L("Permission:Platform.Currencies.Manage"));

        group.AddPermission(PlatformPermissions.TaxRates.Default, L("Permission:Platform.TaxRates"))
            .AddChild(PlatformPermissions.TaxRates.Manage, L("Permission:Platform.TaxRates.Manage"));

        group.AddPermission(PlatformPermissions.BusinessAudit.Default, L("Permission:Platform.BusinessAudit"))
            .AddChild(PlatformPermissions.BusinessAudit.View, L("Permission:Platform.BusinessAudit.View"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<OwneyBeautyResource>(name);
}
