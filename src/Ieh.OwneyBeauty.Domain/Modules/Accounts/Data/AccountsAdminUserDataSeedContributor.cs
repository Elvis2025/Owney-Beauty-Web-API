using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Ieh.OwneyBeauty.Modules.Accounts.Data;

/// <summary>
/// Siembra el administrador por defecto de Owney (reemplaza al <see cref="IdentityDataSeedContributor"/> de ABP,
/// que se quita en <c>OwneyBeautyDomainModule</c>). Los valores del <see cref="DataSeedContext"/> tienen prioridad
/// sobre los de <see cref="OwneyBeautyConsts"/>. Si el usuario ya existe, no se toca su contraseña ni su correo.
/// </summary>
public class AccountsAdminUserDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly IdentityUserManager _userManager;

    public AccountsAdminUserDataSeedContributor(IIdentityDataSeeder identityDataSeeder, IdentityUserManager userManager)
    {
        _identityDataSeeder = identityDataSeeder;
        _userManager = userManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var userName = context[IdentityDataSeedContributor.AdminUserNamePropertyName] as string ?? OwneyBeautyConsts.AdminUserNameDefaultValue;

        await _identityDataSeeder.SeedAsync(
            context[IdentityDataSeedContributor.AdminEmailPropertyName] as string ?? OwneyBeautyConsts.AdminEmailDefaultValue,
            context[IdentityDataSeedContributor.AdminPasswordPropertyName] as string ?? OwneyBeautyConsts.AdminPasswordDefaultValue,
            context.TenantId,
            userName);

        // ABP crea el usuario con Name = nombre de usuario; solo se completa si nadie lo ha cambiado.
        var admin = await _userManager.FindByNameAsync(userName);
        if (admin is null || !string.IsNullOrEmpty(admin.Surname) || (!string.IsNullOrEmpty(admin.Name) && admin.Name != userName))
        {
            return;
        }

        admin.Name = OwneyBeautyConsts.AdminNameDefaultValue;
        admin.Surname = OwneyBeautyConsts.AdminSurnameDefaultValue;
        (await _userManager.UpdateAsync(admin)).CheckErrors();
    }
}
