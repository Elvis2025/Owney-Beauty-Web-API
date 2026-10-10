using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Service;

[AllowAnonymous]
public class GeographyAppService : OwneyBeautyAppService, IGeographyAppService
{
    private readonly IRepository<Province, Guid> _provinceRepository;
    private readonly IRepository<Municipality, Guid> _municipalityRepository;

    public GeographyAppService(IRepository<Province, Guid> provinceRepository, IRepository<Municipality, Guid> municipalityRepository)
    {
        _provinceRepository = provinceRepository;
        _municipalityRepository = municipalityRepository;
    }

    public virtual async Task<ListResultDto<ProvinceDto>> GetProvincesAsync()
    {
        var query = (await _provinceRepository.GetQueryableAsync()).Where(x => x.IsActive).OrderBy(x => x.Name);
        var items = await AsyncExecuter.ToListAsync(query);
        return new ListResultDto<ProvinceDto>(ObjectMapper.Map<List<Province>, List<ProvinceDto>>(items));
    }

    public virtual async Task<ListResultDto<MunicipalityDto>> GetMunicipalitiesAsync(Guid provinceId)
    {
        var query = (await _municipalityRepository.GetQueryableAsync())
            .Where(x => x.ProvinceId == provinceId && x.IsActive)
            .OrderBy(x => x.Name);
        var items = await AsyncExecuter.ToListAsync(query);
        return new ListResultDto<MunicipalityDto>(ObjectMapper.Map<List<Municipality>, List<MunicipalityDto>>(items));
    }
}
