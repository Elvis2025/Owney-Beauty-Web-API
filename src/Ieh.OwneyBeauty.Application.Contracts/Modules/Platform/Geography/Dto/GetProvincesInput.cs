using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

public class GetProvincesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
