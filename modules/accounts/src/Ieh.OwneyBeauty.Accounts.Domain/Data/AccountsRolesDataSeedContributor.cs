using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Ieh.OwneyBeauty.Accounts.Data;

/// <summary>
/// Crea los roles de Owney (ACC-M01). Los permisos de cada rol se asignan desde el panel
/// de administración o desde el seeder de cada módulo cuando el módulo exista.
/// "client" es rol por defecto: todo usuario nuevo puede reservar y usar la red social.
/// </summary>
public class AccountsRolesDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IGuidGenerator _guidGenerator;

    public AccountsRolesDataSeedContributor(IIdentityRoleRepository roleRepository, IdentityRoleManager roleManager, IGuidGenerator guidGenerator)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        foreach (var name in OwneyRoles.All)
        {
            var normalized = _roleManager.NormalizeKey(name);
            if (await _roleRepository.FindByNormalizedNameAsync(normalized) is not null)
            {
                continue;
            }

            var role = new IdentityRole(_guidGenerator.Create(), name, context.TenantId)
            {
                IsStatic = true,
                IsPublic = name is OwneyRoles.Client or OwneyRoles.Professional or OwneyRoles.BusinessOwner,
                IsDefault = name == OwneyRoles.Client,
            };

            (await _roleManager.CreateAsync(role)).CheckErrors();
        }
    }
}
