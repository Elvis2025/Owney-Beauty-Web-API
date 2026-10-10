using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

public class ProvinceDto : AuditedEntityDto<Guid>
{
    public string CountryCode { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}
