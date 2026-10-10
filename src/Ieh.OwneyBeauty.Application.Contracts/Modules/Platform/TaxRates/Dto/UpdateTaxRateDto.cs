using System;
using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

public class UpdateTaxRateDto
{
    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Range(typeof(decimal), "0", "1")]
    public decimal Rate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}
