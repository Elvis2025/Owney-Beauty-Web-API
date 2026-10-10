using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Modules.Platform.Models;

/// <summary>Moneda ISO 4217 habilitada en la plataforma (PLT-M02). DOP es la moneda por defecto.</summary>
public class Currency : AuditedAggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Symbol { get; private set; } = null!;

    public int DecimalPlaces { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; }

    protected Currency()
    {
    }

    public Currency(Guid id, string code, string name, string symbol, int decimalPlaces = 2)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), PlatformConsts.CurrencyCodeLength, PlatformConsts.CurrencyCodeLength).ToUpperInvariant();
        Update(name, symbol, decimalPlaces);
        IsActive = true;
    }

    public Currency Update(string name, string symbol, int decimalPlaces)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), PlatformConsts.MaxNameLength).Trim();
        Symbol = Check.NotNullOrWhiteSpace(symbol, nameof(symbol), PlatformConsts.MaxCurrencySymbolLength).Trim();
        Check.Range(decimalPlaces, nameof(decimalPlaces), 0, 4);
        DecimalPlaces = decimalPlaces;
        return this;
    }

    public Currency SetActive(bool isActive)
    {
        if (!isActive && IsDefault)
        {
            throw new BusinessException(PlatformErrorCodes.CannotDeactivateDefaultCurrency);
        }

        IsActive = isActive;
        return this;
    }

    /// <summary>Solo debe llamarse desde <see cref="CurrencyManager"/>, que garantiza una única moneda por defecto.</summary>
    internal void SetDefault(bool isDefault)
    {
        if (isDefault)
        {
            IsActive = true;
        }

        IsDefault = isDefault;
    }
}
