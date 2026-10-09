using Ieh.OwneyBeauty.SharedKernel;
using Volo.Abp.Localization;

namespace Ieh.OwneyBeauty.Platform.Localization
{
    [LocalizationResourceName("Platform")]
    public class PlatformResource
    {
    }
}

namespace Ieh.OwneyBeauty.Platform
{
    public static class PlatformDbProperties
    {
        public const string DbSchema = "platform";

        public const string DbTablePrefix = "";

        public const string ConnectionStringName = "Platform";
    }

    public static class PlatformErrorCodes
    {
        public const string Namespace = OwneyErrorCodes.Prefix + ".Platform";

        public const string DuplicateCode = Namespace + ":DuplicateCode";
        public const string CannotDeactivateDefaultCurrency = Namespace + ":CannotDeactivateDefaultCurrency";
        public const string InvalidTaxRate = Namespace + ":InvalidTaxRate";
        public const string IdempotencyKeyReused = Namespace + ":IdempotencyKeyReused";
        public const string IdempotentRequestInProgress = Namespace + ":IdempotentRequestInProgress";
    }

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

    public static class PlatformSettingNames
    {
        private const string Prefix = "Owney.Platform";

        public const string Market = Prefix + ".Market";
        public const string DefaultCurrency = Prefix + ".DefaultCurrency";
        public const string DefaultTimeZone = Prefix + ".DefaultTimeZone";
        public const string IdempotencyRetentionHours = Prefix + ".IdempotencyRetentionHours";
    }
}
