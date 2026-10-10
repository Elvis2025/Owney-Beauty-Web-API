namespace Ieh.OwneyBeauty.Modules.Accounts;

/// <summary>Roles del sistema (ACC-M01). "admin" es el rol que ABP crea por defecto.</summary>
public static class OwneyRoles
{
    public const string Admin = "admin";
    public const string Client = "client";
    public const string Professional = "professional";
    public const string BusinessOwner = "business-owner";
    public const string Employee = "employee";
    public const string KycAnalyst = "kyc-analyst";
    public const string Support = "support";
    public const string Moderator = "moderator";
    public const string Finance = "finance";
    public const string Marketing = "marketing";

    public static readonly string[] All =
    {
        Client, Professional, BusinessOwner, Employee, KycAnalyst, Support, Moderator, Finance, Marketing,
    };

    /// <summary>Roles internos de alto riesgo: MFA obligatorio antes del go-live (ACC-F10).</summary>
    public static readonly string[] Internal = { Admin, KycAnalyst, Support, Moderator, Finance, Marketing };
}
