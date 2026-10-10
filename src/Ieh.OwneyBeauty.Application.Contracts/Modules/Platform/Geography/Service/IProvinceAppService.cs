using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Service;

/// <summary>Mantenimiento de provincias (Admin). No se eliminan: se desactivan.</summary>
public interface IProvinceAppService : IApplicationService
{
    Task<PagedResultDto<ProvinceDto>> GetListAsync(GetProvincesInput input);
    Task<ProvinceDto> GetAsync(Guid id);
    Task<ProvinceDto> CreateAsync(CreateUpdateProvinceDto input);
    Task<ProvinceDto> UpdateAsync(Guid id, CreateUpdateProvinceDto input);
}
