namespace Ieh.OwneyBeauty.Modules.Accounts;

public static class AccountsPermissions
{
    public const string GroupName = "Accounts";

    public static class LegalDocuments
    {
        public const string Default = GroupName + ".LegalDocuments";
        public const string Manage = Default + ".Manage";
    }
}
