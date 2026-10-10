using System;
using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

public class CreateUpdateMunicipalityDto
{
    [Required]
    public Guid ProvinceId { get; set; }

    [Required, StringLength(PlatformConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
