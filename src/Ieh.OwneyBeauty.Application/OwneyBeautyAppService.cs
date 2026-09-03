using Ieh.OwneyBeauty.Localization;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty;

/* Inherit your application services from this class.
 */
public abstract class OwneyBeautyAppService : ApplicationService
{
    protected OwneyBeautyAppService()
    {
        LocalizationResource = typeof(OwneyBeautyResource);
    }
}
