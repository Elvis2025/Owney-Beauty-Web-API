namespace Ieh.OwneyBeauty.Modules.Platform;

public static class PlatformConsts
{
    public const int MaxCodeLength = 16;
    public const int MaxNameLength = 128;
    public const int CurrencyCodeLength = 3;
    public const int MaxCurrencySymbolLength = 8;
    public const int CountryCodeLength = 2;

    public const int MaxIdempotencyKeyLength = 128;
    public const int MaxIdempotencyScopeLength = 64;
    public const int MaxRequestPathLength = 512;
    public const int RequestHashLength = 64;
    public const int MaxResponseContentTypeLength = 128;

    public const int MaxAuditModuleLength = 64;
    public const int MaxAuditActionLength = 128;
    public const int MaxAuditEntityTypeLength = 128;
    public const int MaxAuditEntityIdLength = 64;
    public const int MaxAuditReasonLength = 1024;
    public const int MaxAuditUserNameLength = 256;
    public const int MaxAuditClientIpLength = 64;
    public const int MaxAuditCorrelationIdLength = 64;

    public const string DefaultMarket = "DO";
    public const string DefaultTimeZone = "America/Santo_Domingo";
}
