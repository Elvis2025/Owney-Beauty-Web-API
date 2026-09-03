using Ieh.OwneyBeauty.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Ieh.OwneyBeauty.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class OwneyBeautyController : AbpControllerBase
{
    protected OwneyBeautyController()
    {
        LocalizationResource = typeof(OwneyBeautyResource);
    }
}
