using Ieh.OwneyBeauty.Common;
namespace Ieh.OwneyBeauty.Modules.Platform;

public static class PlatformErrorCodes
{
    public const string Namespace = OwneyErrorCodes.Prefix + ".Platform";

    public const string DuplicateCode = Namespace + ":DuplicateCode";
    public const string CannotDeactivateDefaultCurrency = Namespace + ":CannotDeactivateDefaultCurrency";
    public const string InvalidTaxRate = Namespace + ":InvalidTaxRate";
    public const string IdempotencyKeyReused = Namespace + ":IdempotencyKeyReused";
    public const string IdempotentRequestInProgress = Namespace + ":IdempotentRequestInProgress";
}
