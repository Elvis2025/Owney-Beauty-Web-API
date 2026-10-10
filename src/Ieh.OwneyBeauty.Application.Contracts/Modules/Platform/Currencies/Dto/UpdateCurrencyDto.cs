using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Platform.Currencies.Dto;

public class UpdateCurrencyDto
{
    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxCurrencySymbolLength)]
    public string Symbol { get; set; } = null!;

    [Range(0, 4)]
    public int DecimalPlaces { get; set; } = 2;

    public bool IsActive { get; set; } = true;
}
