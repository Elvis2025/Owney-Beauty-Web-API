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
using Ieh.OwneyBeauty.Modules.Platform.Models;
using Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Service;

[Authorize(PlatformPermissions.TaxRates.Manage)]
public class TaxRateAppService : OwneyBeautyAppService, ITaxRateAppService
{
    private readonly IRepository<TaxRate, Guid> _repository;
    private readonly IBusinessAuditLogger _audit;

    public TaxRateAppService(IRepository<TaxRate, Guid> repository, IBusinessAuditLogger audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public virtual async Task<PagedResultDto<TaxRateDto>> GetListAsync(GetTaxRatesInput input)
    {
        var query = (await _repository.GetQueryableAsync())
            .WhereIf(!input.CountryCode.IsNullOrWhiteSpace(), x => x.CountryCode == input.CountryCode!.ToUpper())
            .WhereIf(!input.Code.IsNullOrWhiteSpace(), x => x.Code == input.Code!.ToUpper());

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Code, EffectiveFrom desc" : input.Sorting!)
            .PageBy(input));
        return new PagedResultDto<TaxRateDto>(total, ObjectMapper.Map<List<TaxRate>, List<TaxRateDto>>(items));
    }

    public virtual async Task<TaxRateDto> GetAsync(Guid id) =>
        ObjectMapper.Map<TaxRate, TaxRateDto>(await _repository.GetAsync(id));

    public virtual async Task<TaxRateDto> CreateAsync(CreateTaxRateDto input)
    {
        var code = input.Code.Trim().ToUpperInvariant();
        var country = input.CountryCode.Trim().ToUpperInvariant();
        var from = Clock.Normalize(input.EffectiveFrom);
        if (await _repository.AnyAsync(x => x.CountryCode == country && x.Code == code && x.EffectiveFrom == from))
        {
            throw new BusinessException(PlatformErrorCodes.DuplicateCode).WithData("code", code);
        }

        var taxRate = new TaxRate(GuidGenerator.Create(), country, code, input.Name, input.Rate, from,
            input.EffectiveTo.HasValue ? Clock.Normalize(input.EffectiveTo.Value) : null);
        await _repository.InsertAsync(taxRate, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "TaxRate.Created", nameof(TaxRate), taxRate.Id.ToString(),
            Data: new Dictionary<string, string> { ["rate"] = taxRate.Rate.ToString(System.Globalization.CultureInfo.InvariantCulture) }));
        return ObjectMapper.Map<TaxRate, TaxRateDto>(taxRate);
    }

    public virtual async Task<TaxRateDto> UpdateAsync(Guid id, UpdateTaxRateDto input)
    {
        var taxRate = await _repository.GetAsync(id);
        taxRate.Update(input.Name, input.Rate, Clock.Normalize(input.EffectiveFrom),
                input.EffectiveTo.HasValue ? Clock.Normalize(input.EffectiveTo.Value) : null)
            .SetActive(input.IsActive);
        await _repository.UpdateAsync(taxRate, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "TaxRate.Updated", nameof(TaxRate), taxRate.Id.ToString(),
            Data: new Dictionary<string, string> { ["rate"] = taxRate.Rate.ToString(System.Globalization.CultureInfo.InvariantCulture) }));
        return ObjectMapper.Map<TaxRate, TaxRateDto>(taxRate);
    }

    [Authorize]
    public virtual async Task<TaxRateDto?> GetEffectiveAsync(string countryCode, string code, DateTime? at = null)
    {
        var instant = at.HasValue ? Clock.Normalize(at.Value) : Clock.Now;
        var country = countryCode.Trim().ToUpperInvariant();
        var normalizedCode = code.Trim().ToUpperInvariant();
        var candidates = await _repository.GetListAsync(x => x.CountryCode == country && x.Code == normalizedCode && x.IsActive);
        var effective = candidates
            .Where(x => x.IsEffectiveAt(instant))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();
        return effective is null ? null : ObjectMapper.Map<TaxRate, TaxRateDto>(effective);
    }
}
