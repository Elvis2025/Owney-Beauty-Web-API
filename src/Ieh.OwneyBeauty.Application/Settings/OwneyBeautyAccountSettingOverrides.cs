using Volo.Abp.Account.Settings;
using Volo.Abp.Settings;

namespace Ieh.OwneyBeauty.Settings;

/// <summary>
/// El registro de usuarios se hace solo por /api/accounts/registration/register (módulo Accounts),
/// que exige aceptar los documentos legales vigentes y asigna roles y perfil. Se desactiva el
/// auto-registro nativo de ABP para que no exista un camino alternativo sin esas garantías.
/// </summary>
public class OwneyBeautyAccountSettingOverrides : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var selfRegistration = context.GetOrNull(AccountSettingNames.IsSelfRegistrationEnabled);
        if (selfRegistration != null)
        {
            selfRegistration.DefaultValue = false.ToString();
        }
    }
}
