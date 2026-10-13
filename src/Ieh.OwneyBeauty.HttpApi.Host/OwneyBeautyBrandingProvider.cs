using Microsoft.Extensions.Localization;
using Ieh.OwneyBeauty.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Ieh.OwneyBeauty;

[Dependency(ReplaceServices = true)]
public class OwneyBeautyBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<OwneyBeautyResource> _localizer;

    public OwneyBeautyBrandingProvider(IStringLocalizer<OwneyBeautyResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];

    public override string LogoUrl => "/images/logo/leptonxlite/logo-light.png";

    public override string LogoReverseUrl => "/images/logo/leptonxlite/logo-dark.png";
}
