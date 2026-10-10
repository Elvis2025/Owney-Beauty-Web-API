using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

public class TaxRateDto : AuditedEntityDto<Guid>
{
    public string CountryCode { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
