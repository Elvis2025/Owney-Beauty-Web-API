using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty;

/* Inherit from this class for your domain layer tests. */
public abstract class OwneyBeautyDomainTestBase<TStartupModule> : OwneyBeautyTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
