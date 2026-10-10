using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Ieh.OwneyBeauty.Localization;

namespace Ieh.OwneyBeauty.Modules.Accounts;

public class AccountsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(AccountsPermissions.GroupName, L("Permission:Accounts"));
        group.AddPermission(AccountsPermissions.LegalDocuments.Default, L("Permission:Accounts.LegalDocuments"))
            .AddChild(AccountsPermissions.LegalDocuments.Manage, L("Permission:Accounts.LegalDocuments.Manage"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<OwneyBeautyResource>(name);
}
