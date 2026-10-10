using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.Currencies.Dto;

public class CurrencyDto : AuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public int DecimalPlaces { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}
