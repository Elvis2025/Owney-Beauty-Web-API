using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

public class MunicipalityDto : AuditedEntityDto<Guid>
{
    public Guid ProvinceId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}
