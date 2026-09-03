using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty;

[DependsOn(
    typeof(OwneyBeautyApplicationModule),
    typeof(OwneyBeautyDomainTestModule)
)]
public class OwneyBeautyApplicationTestModule : AbpModule
{

}
