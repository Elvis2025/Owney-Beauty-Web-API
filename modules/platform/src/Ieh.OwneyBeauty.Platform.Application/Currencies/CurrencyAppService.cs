using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ieh.OwneyBeauty.Platform.Permissions;
using Ieh.OwneyBeauty.SharedKernel.Auditing;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Ieh.OwneyBeauty.Platform.Currencies;

[Authorize(PlatformPermissions.Currencies.Manage)]
public class CurrencyAppService : PlatformAppService, ICurrencyAppService
{
    private readonly IRepository<Currency, Guid> _repository;
    private readonly CurrencyManager _currencyManager;
    private readonly IBusinessAuditLogger _audit;

    public CurrencyAppService(IRepository<Currency, Guid> repository, CurrencyManager currencyManager, IBusinessAuditLogger audit)
    {
        _repository = repository;
        _currencyManager = currencyManager;
        _audit = audit;
    }

    [AllowAnonymous]
    public virtual async Task<ListResultDto<CurrencyDto>> GetActiveAsync()
    {
        var query = (await _repository.GetQueryableAsync()).Where(x => x.IsActive).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code);
        return new ListResultDto<CurrencyDto>(ObjectMapper.Map<List<Currency>, List<CurrencyDto>>(await AsyncExecuter.ToListAsync(query)));
    }

    public virtual async Task<ListResultDto<CurrencyDto>> GetListAsync()
    {
        var query = (await _repository.GetQueryableAsync()).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code);
        return new ListResultDto<CurrencyDto>(ObjectMapper.Map<List<Currency>, List<CurrencyDto>>(await AsyncExecuter.ToListAsync(query)));
    }

    public virtual async Task<CurrencyDto> GetAsync(Guid id) =>
        ObjectMapper.Map<Currency, CurrencyDto>(await _repository.GetAsync(id));

    public virtual async Task<CurrencyDto> CreateAsync(CreateCurrencyDto input)
    {
        var currency = await _currencyManager.CreateAsync(input.Code, input.Name, input.Symbol, input.DecimalPlaces);
        await _repository.InsertAsync(currency, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Currency.Created", nameof(Currency), currency.Id.ToString()));
        return ObjectMapper.Map<Currency, CurrencyDto>(currency);
    }

    public virtual async Task<CurrencyDto> UpdateAsync(Guid id, UpdateCurrencyDto input)
    {
        var currency = await _repository.GetAsync(id);
        currency.Update(input.Name, input.Symbol, input.DecimalPlaces).SetActive(input.IsActive);
        await _repository.UpdateAsync(currency, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Currency.Updated", nameof(Currency), currency.Id.ToString()));
        return ObjectMapper.Map<Currency, CurrencyDto>(currency);
    }

    public virtual async Task<CurrencyDto> SetDefaultAsync(Guid id)
    {
        var currency = await _repository.GetAsync(id);
        await _currencyManager.SetDefaultAsync(currency);
        await _repository.UpdateAsync(currency, autoSave: true);
        await _audit.LogAsync(new BusinessAuditEntry("Platform", "Currency.SetDefault", nameof(Currency), currency.Id.ToString()));
        return ObjectMapper.Map<Currency, CurrencyDto>(currency);
    }
}
