using Ieh.OwneyBeauty.Platform.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Reflection;

namespace Ieh.OwneyBeauty.Platform.Permissions;

public static class PlatformPermissions
{
    public const string GroupName = "Platform";

    public static class Geography
    {
        public const string Default = GroupName + ".Geography";
        public const string Manage = Default + ".Manage";
    }

    public static class Currencies
    {
        public const string Default = GroupName + ".Currencies";
        public const string Manage = Default + ".Manage";
    }

    public static class TaxRates
    {
        public const string Default = GroupName + ".TaxRates";
        public const string Manage = Default + ".Manage";
    }

    public static class BusinessAudit
    {
        public const string Default = GroupName + ".BusinessAudit";
        public const string View = Default + ".View";
    }

    public static string[] GetAll() => ReflectionHelper.GetPublicConstantsRecursively(typeof(PlatformPermissions));
}

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

    private static LocalizableString L(string name) => LocalizableString.Create<PlatformResource>(name);
}
