using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Features;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Platform;

[DependsOn(
    typeof(PlatformDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationAbstractionsModule),
    typeof(AbpFeaturesModule)
)]
public class PlatformApplicationContractsModule : AbpModule
{
}
