using Ieh.OwneyBeauty.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Ieh.OwneyBeauty.Permissions;

public class OwneyBeautyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(OwneyBeautyPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(OwneyBeautyPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(OwneyBeautyPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(OwneyBeautyPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(OwneyBeautyPermissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(OwneyBeautyPermissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(OwneyBeautyPermissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(OwneyBeautyPermissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(OwneyBeautyPermissions.Authors.Delete, L("Permission:Authors.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(OwneyBeautyPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<OwneyBeautyResource>(name);
    }
}
