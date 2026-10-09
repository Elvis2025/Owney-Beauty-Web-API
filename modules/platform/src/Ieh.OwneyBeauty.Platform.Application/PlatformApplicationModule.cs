using Microsoft.Extensions.DependencyInjection;
using Ieh.OwneyBeauty.Platform.Localization;
using Volo.Abp.Application;
using Volo.Abp.Application.Services;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace Ieh.OwneyBeauty.Platform;

[DependsOn(
    typeof(PlatformDomainModule),
    typeof(PlatformApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class PlatformApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<PlatformApplicationModule>();
    }
}

public abstract class PlatformAppService : ApplicationService
{
    protected PlatformAppService()
    {
        LocalizationResource = typeof(PlatformResource);
        ObjectMapperContext = typeof(PlatformApplicationModule);
    }
}
