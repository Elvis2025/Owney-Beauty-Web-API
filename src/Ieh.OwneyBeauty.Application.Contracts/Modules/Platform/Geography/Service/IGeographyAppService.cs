using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Service;

/// <summary>Lecturas públicas (sin autenticación) para formularios de dirección y filtros.</summary>
public interface IGeographyAppService : IApplicationService
{
    Task<ListResultDto<ProvinceDto>> GetProvincesAsync();

    Task<ListResultDto<MunicipalityDto>> GetMunicipalitiesAsync(Guid provinceId);
}
