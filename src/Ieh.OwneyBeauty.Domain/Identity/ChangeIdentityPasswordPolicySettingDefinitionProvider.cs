using Volo.Abp.Identity.Settings;
using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Identity;

/// <summary>
/// ACC-M04: política de contraseñas y bloqueo de Owney.
/// Mínimo 8 caracteres con al menos un dígito y una minúscula; bloqueo de 15 minutos tras 5 intentos fallidos.
/// Todos los valores se pueden ajustar desde Administración sin redesplegar.
/// </summary>
public class ChangeIdentityPasswordPolicySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        SetDefault(context, IdentitySettingNames.Password.RequiredLength, "8");
        SetDefault(context, IdentitySettingNames.Password.RequireDigit, true.ToString());
        SetDefault(context, IdentitySettingNames.Password.RequireLowercase, true.ToString());
        SetDefault(context, IdentitySettingNames.Password.RequireUppercase, false.ToString());
        SetDefault(context, IdentitySettingNames.Password.RequireNonAlphanumeric, false.ToString());

        SetDefault(context, IdentitySettingNames.Lockout.AllowedForNewUsers, true.ToString());
        SetDefault(context, IdentitySettingNames.Lockout.MaxFailedAccessAttempts, "5");
        SetDefault(context, IdentitySettingNames.Lockout.LockoutDuration, "900");
    }

    private static void SetDefault(ISettingDefinitionContext context, string name, string value)
    {
        var definition = context.GetOrNull(name);
        if (definition != null)
        {
            definition.DefaultValue = value;
        }
    }
}
