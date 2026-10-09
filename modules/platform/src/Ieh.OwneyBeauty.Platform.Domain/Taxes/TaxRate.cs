using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Platform.Taxes;

/// <summary>Tasa de impuesto con vigencia (PLT-M03). Ej.: ITBIS 18 % en República Dominicana.</summary>
public class TaxRate : AuditedAggregateRoot<Guid>
{
    public string CountryCode { get; private set; } = PlatformConsts.DefaultMarket;

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    /// <summary>Fracción decimal: 0.18 = 18 %.</summary>
    public decimal Rate { get; private set; }

    public DateTime EffectiveFrom { get; private set; }

    public DateTime? EffectiveTo { get; private set; }

    public bool IsActive { get; private set; }

    protected TaxRate()
    {
    }

    public TaxRate(Guid id, string countryCode, string code, string name, decimal rate, DateTime effectiveFrom, DateTime? effectiveTo = null)
        : base(id)
    {
        CountryCode = Check.NotNullOrWhiteSpace(countryCode, nameof(countryCode), PlatformConsts.CountryCodeLength, PlatformConsts.CountryCodeLength).ToUpperInvariant();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), PlatformConsts.MaxCodeLength).Trim().ToUpperInvariant();
        Update(name, rate, effectiveFrom, effectiveTo);
        IsActive = true;
    }

    public TaxRate Update(string name, decimal rate, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new BusinessException(PlatformErrorCodes.InvalidTaxRate).WithData("rate", rate);
        }

        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
        {
            throw new BusinessException(SharedKernel.OwneyErrorCodes.Shared.InvalidDateRange);
        }

        Name = Check.NotNullOrWhiteSpace(name, nameof(name), PlatformConsts.MaxNameLength).Trim();
        Rate = rate;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        return this;
    }

    public TaxRate SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }

    public bool IsEffectiveAt(DateTime instant) =>
        IsActive && instant >= EffectiveFrom && (!EffectiveTo.HasValue || instant < EffectiveTo.Value);
}
