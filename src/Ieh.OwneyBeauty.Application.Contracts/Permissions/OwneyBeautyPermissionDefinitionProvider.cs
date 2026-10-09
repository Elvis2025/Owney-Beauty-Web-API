using Ieh.OwneyBeauty.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Ieh.OwneyBeauty.Permissions;

public class OwneyBeautyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(OwneyBeautyPermissions.GroupName, L("Permission:OwneyBeauty"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<OwneyBeautyResource>(name);
    }
}
