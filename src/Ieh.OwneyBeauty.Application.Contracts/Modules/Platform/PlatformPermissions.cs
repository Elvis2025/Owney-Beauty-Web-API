using Volo.Abp.Reflection;

namespace Ieh.OwneyBeauty.Modules.Platform;

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
