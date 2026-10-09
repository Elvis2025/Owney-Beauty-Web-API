using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty.Platform.Geography;

public class ProvinceDto : AuditedEntityDto<Guid>
{
    public string CountryCode { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class MunicipalityDto : AuditedEntityDto<Guid>
{
    public Guid ProvinceId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateUpdateProvinceDto
{
    [Required, StringLength(PlatformConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

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

public class GetProvincesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}

public class GetMunicipalitiesInput : PagedAndSortedResultRequestDto
{
    public Guid? ProvinceId { get; set; }
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Lecturas públicas (sin autenticación) para formularios de dirección y filtros.</summary>
public interface IGeographyAppService : IApplicationService
{
    Task<ListResultDto<ProvinceDto>> GetProvincesAsync();

    Task<ListResultDto<MunicipalityDto>> GetMunicipalitiesAsync(Guid provinceId);
}

/// <summary>Mantenimiento de provincias (Admin). No se eliminan: se desactivan.</summary>
public interface IProvinceAppService : IApplicationService
{
    Task<PagedResultDto<ProvinceDto>> GetListAsync(GetProvincesInput input);
    Task<ProvinceDto> GetAsync(Guid id);
    Task<ProvinceDto> CreateAsync(CreateUpdateProvinceDto input);
    Task<ProvinceDto> UpdateAsync(Guid id, CreateUpdateProvinceDto input);
}

/// <summary>Mantenimiento de municipios (Admin). No se eliminan: se desactivan.</summary>
public interface IMunicipalityAppService : IApplicationService
{
    Task<PagedResultDto<MunicipalityDto>> GetListAsync(GetMunicipalitiesInput input);
    Task<MunicipalityDto> GetAsync(Guid id);
    Task<MunicipalityDto> CreateAsync(CreateUpdateMunicipalityDto input);
    Task<MunicipalityDto> UpdateAsync(Guid id, CreateUpdateMunicipalityDto input);
}
