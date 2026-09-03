using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty;

public abstract class OwneyBeautyApplicationTestBase<TStartupModule> : OwneyBeautyTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
