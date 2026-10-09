using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Ieh.OwneyBeauty.Platform.Currencies;

public class CurrencyManager : DomainService
{
    private readonly IRepository<Currency, Guid> _currencyRepository;

    public CurrencyManager(IRepository<Currency, Guid> currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Currency> CreateAsync(string code, string name, string symbol, int decimalPlaces)
    {
        var normalized = code.Trim().ToUpperInvariant();
        if (await _currencyRepository.AnyAsync(x => x.Code == normalized))
        {
            throw new BusinessException(PlatformErrorCodes.DuplicateCode).WithData("code", normalized);
        }

        return new Currency(GuidGenerator.Create(), normalized, name, symbol, decimalPlaces);
    }

    /// <summary>Marca la moneda como predeterminada y desmarca la anterior (solo una puede serlo).</summary>
    public async Task SetDefaultAsync(Currency currency)
    {
        var currentDefaults = await _currencyRepository.GetListAsync(x => x.IsDefault && x.Id != currency.Id);
        foreach (var previous in currentDefaults)
        {
            previous.SetDefault(false);
            await _currencyRepository.UpdateAsync(previous);
        }

        currency.SetDefault(true);
    }
}
