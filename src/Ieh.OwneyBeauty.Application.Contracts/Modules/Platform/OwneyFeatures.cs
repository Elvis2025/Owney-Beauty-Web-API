namespace Ieh.OwneyBeauty.Modules.Platform;

/// <summary>
/// Interruptores por módulo (PLT-M05). Permiten activar funciones de forma gradual y apagarlas
/// en caso de incidente sin redesplegar (kill-switch, §16.2 del documento maestro).
/// </summary>
public static class OwneyFeatures
{
    public const string GroupName = "Owney";

    public const string Social = GroupName + ".Social";
    public const string Bookings = GroupName + ".Bookings";
    public const string HomeService = GroupName + ".HomeService";
    public const string Payments = GroupName + ".Payments";
    public const string Messaging = GroupName + ".Messaging";
}
