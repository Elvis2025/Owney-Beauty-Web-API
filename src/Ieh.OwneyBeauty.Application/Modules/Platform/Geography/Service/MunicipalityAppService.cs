using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Ieh.OwneyBeauty.Common.Auditing;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Service;

[Authorize(PlatformPermissions.Geography.Manage)]
public class MunicipalityAppService : OwneyBeautyAppService, IMunicipalityAppService
{
    private readonly IRepository<Municipality, Guid> _repository;
    private readonly IRepository<Province, Guid> _provinceRepository;
    private readonly IBusinessAuditLogger _audit;

    public MunicipalityAppService(IRepository<Municipality, Guid> repository, IRepository<Province, Guid> provinceRepository, IBusinessAuditLogger audit)
    {
        _repository = repository;
        _provinceRepository = provinceRepository;
        _audit = audit;
    }

    public virtual async Task<PagedResultDto<MunicipalityDto>> GetListAsync(GetMunicipalitiesInput input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(input.ProvinceId.HasValue, x => x.ProvinceId == input.ProvinceId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Municipality.Name) : input.Sorting!)
            .PageBy(input));
        return new PagedResultDto<MunicipalityDto>(total, ObjectMapper.Map<List<Municipality>, List<MunicipalityDto>>(items));
    }

    public virtual async Task<MunicipalityDto> GetAsync(Guid id) =>
        ObjectMapper.Map<Municipality, MunicipalityDto>(await _repository.GetAsync(id));

    public virtual async Task<MunicipalityDto> CreateAsync(CreateUpdateMunicipalityDto input)
    {
        await _provinceRepository.GetAsync(input.ProvinceId);
        await EnsureUniqueCodeAsync(input.Code, null);
        var municipality = new Municipality(GuidGenerator.Create(), input.ProvinceId, input.Code, input.Name).SetActive(input.IsActive);
        await _repository.InsertAsync(municipality, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Municipality.Created", nameof(Municipality), municipality.Id.ToString()));
        return ObjectMapper.Map<Municipality, MunicipalityDto>(municipality);
    }

    public virtual async Task<MunicipalityDto> UpdateAsync(Guid id, CreateUpdateMunicipalityDto input)
    {
        var municipality = await _repository.GetAsync(id);
        await _provinceRepository.GetAsync(input.ProvinceId);
        await EnsureUniqueCodeAsync(input.Code, id);
        municipality.SetProvince(input.ProvinceId).SetCode(input.Code).SetName(input.Name).SetActive(input.IsActive);
        await _repository.UpdateAsync(municipality, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Municipality.Updated", nameof(Municipality), municipality.Id.ToString()));
        return ObjectMapper.Map<Municipality, MunicipalityDto>(municipality);
    }

    private async Task EnsureUniqueCodeAsync(string code, Guid? excludeId)
    {
        var normalized = code.Trim();
        if (await _repository.AnyAsync(x => x.Code == normalized && x.Id != excludeId))
        {
            throw new BusinessException(PlatformErrorCodes.DuplicateCode).WithData("code", normalized);
        }
    }
}
