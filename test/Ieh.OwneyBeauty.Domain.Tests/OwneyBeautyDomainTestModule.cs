using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty;

[DependsOn(
    typeof(OwneyBeautyDomainModule),
    typeof(OwneyBeautyTestBaseModule)
)]
public class OwneyBeautyDomainTestModule : AbpModule
{

}
