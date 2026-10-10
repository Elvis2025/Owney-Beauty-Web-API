using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Service;

/// <summary>Mantenimiento de municipios (Admin). No se eliminan: se desactivan.</summary>
public interface IMunicipalityAppService : IApplicationService
{
    Task<PagedResultDto<MunicipalityDto>> GetListAsync(GetMunicipalitiesInput input);
    Task<MunicipalityDto> GetAsync(Guid id);
    Task<MunicipalityDto> CreateAsync(CreateUpdateMunicipalityDto input);
    Task<MunicipalityDto> UpdateAsync(Guid id, CreateUpdateMunicipalityDto input);
}
