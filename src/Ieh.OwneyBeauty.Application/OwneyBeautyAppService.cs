using Ieh.OwneyBeauty.Localization;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty;

/* Base de todos los app services (también los de Modules/*). */
public abstract class OwneyBeautyAppService : ApplicationService
{
    protected OwneyBeautyAppService()
    {
        LocalizationResource = typeof(OwneyBeautyResource);
        ObjectMapperContext = typeof(OwneyBeautyApplicationModule);
    }
}
