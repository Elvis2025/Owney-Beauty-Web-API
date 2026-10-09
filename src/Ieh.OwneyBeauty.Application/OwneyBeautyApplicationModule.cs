using Ieh.OwneyBeauty.Accounts;
using Ieh.OwneyBeauty.Platform;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.TenantManagement;

namespace Ieh.OwneyBeauty;

[DependsOn(
    typeof(OwneyBeautyDomainModule),
    typeof(OwneyBeautyApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(PlatformApplicationModule),
    typeof(AccountsApplicationModule)
    )]
public class OwneyBeautyApplicationModule : AbpModule
{

}
