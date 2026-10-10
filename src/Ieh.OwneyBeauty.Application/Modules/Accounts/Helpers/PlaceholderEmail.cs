using System;

namespace Ieh.OwneyBeauty.Modules.Accounts.Helpers;

/// <summary>
/// Correo interno para cuentas registradas solo con teléfono. ABP Identity exige un correo;
/// el dominio ".invalid" está reservado (RFC 2606) y nunca entrega mensajes. Nunca se expone al cliente.
/// </summary>
public static class PlaceholderEmail
{
    public const string Domain = "phone.owneybeauty.invalid";

    public static string For(Guid userId) => $"{userId:N}@{Domain}";

    public static bool Is(string? email) =>
        email is not null && email.EndsWith("@" + Domain, StringComparison.OrdinalIgnoreCase);

    public static string? Public(string? email) => Is(email) ? null : email;
}
