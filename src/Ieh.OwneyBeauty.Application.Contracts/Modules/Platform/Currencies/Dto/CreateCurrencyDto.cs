using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Platform.Currencies.Dto;

public class CreateCurrencyDto
{
    [Required, StringLength(PlatformConsts.CurrencyCodeLength, MinimumLength = PlatformConsts.CurrencyCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxCurrencySymbolLength)]
    public string Symbol { get; set; } = null!;

    [Range(0, 4)]
    public int DecimalPlaces { get; set; } = 2;
}
