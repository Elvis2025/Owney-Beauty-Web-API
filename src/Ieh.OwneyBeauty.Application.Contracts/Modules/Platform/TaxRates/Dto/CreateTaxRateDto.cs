using System;
using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

public class CreateTaxRateDto
{
    [Required, StringLength(PlatformConsts.CountryCodeLength, MinimumLength = PlatformConsts.CountryCodeLength)]
    public string CountryCode { get; set; } = PlatformConsts.DefaultMarket;

    [Required, StringLength(PlatformConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    /// <summary>Fracción: 0.18 = 18 %.</summary>
    [Range(typeof(decimal), "0", "1")]
    public decimal Rate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }
}
