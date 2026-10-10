namespace Ieh.OwneyBeauty.Common;

/// <summary>
/// Convención de códigos de error: <c>Owney.{Módulo}:{Código}</c>.
/// Cada módulo mapea su namespace a su recurso de localización (es/en).
/// </summary>
public static class OwneyErrorCodes
{
    public const string Prefix = "Owney";

    public static string For(string module, string code) => $"{Prefix}.{module}:{code}";

    public static class Shared
    {
        public const string Namespace = Prefix + ".Shared";
        public const string CurrencyMismatch = Namespace + ":CurrencyMismatch";
        public const string InvalidCoordinates = Namespace + ":InvalidCoordinates";
        public const string InvalidDateRange = Namespace + ":InvalidDateRange";
        public const string DateTimeMustBeUtc = Namespace + ":DateTimeMustBeUtc";
    }
}
