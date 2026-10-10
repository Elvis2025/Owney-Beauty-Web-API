using System;
using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

public class GetMunicipalitiesInput : PagedAndSortedResultRequestDto
{
    public Guid? ProvinceId { get; set; }
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
