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
public class ProvinceAppService : OwneyBeautyAppService, IProvinceAppService
{
    private readonly IRepository<Province, Guid> _repository;
    private readonly IBusinessAuditLogger _audit;

    public ProvinceAppService(IRepository<Province, Guid> repository, IBusinessAuditLogger audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public virtual async Task<PagedResultDto<ProvinceDto>> GetListAsync(GetProvincesInput input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Province.Code) : input.Sorting!)
            .PageBy(input));
        return new PagedResultDto<ProvinceDto>(total, ObjectMapper.Map<List<Province>, List<ProvinceDto>>(items));
    }

    public virtual async Task<ProvinceDto> GetAsync(Guid id) =>
        ObjectMapper.Map<Province, ProvinceDto>(await _repository.GetAsync(id));

    public virtual async Task<ProvinceDto> CreateAsync(CreateUpdateProvinceDto input)
    {
        await EnsureUniqueCodeAsync(input.Code, null);
        var province = new Province(GuidGenerator.Create(), input.Code, input.Name).SetActive(input.IsActive);
        await _repository.InsertAsync(province, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Province.Created", nameof(Province), province.Id.ToString()));
        return ObjectMapper.Map<Province, ProvinceDto>(province);
    }

    public virtual async Task<ProvinceDto> UpdateAsync(Guid id, CreateUpdateProvinceDto input)
    {
        var province = await _repository.GetAsync(id);
        await EnsureUniqueCodeAsync(input.Code, id);
        province.SetCode(input.Code).SetName(input.Name).SetActive(input.IsActive);
        await _repository.UpdateAsync(province, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Province.Updated", nameof(Province), province.Id.ToString()));
        return ObjectMapper.Map<Province, ProvinceDto>(province);
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
