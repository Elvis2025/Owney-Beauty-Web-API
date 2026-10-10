using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Modules.Platform.Models;

/// <summary>Provincia (o Distrito Nacional). Dato maestro para cobertura, búsqueda y direcciones (PLT-M01).</summary>
public class Province : AuditedAggregateRoot<Guid>
{
    public string CountryCode { get; private set; } = PlatformConsts.DefaultMarket;

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    protected Province()
    {
    }

    public Province(Guid id, string code, string name, string countryCode = PlatformConsts.DefaultMarket)
        : base(id)
    {
        CountryCode = Check.NotNullOrWhiteSpace(countryCode, nameof(countryCode), PlatformConsts.CountryCodeLength, PlatformConsts.CountryCodeLength).ToUpperInvariant();
        SetCode(code);
        SetName(name);
        IsActive = true;
    }

    public Province SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), PlatformConsts.MaxCodeLength).Trim();
        return this;
    }

    public Province SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), PlatformConsts.MaxNameLength).Trim();
        return this;
    }

    public Province SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }
}
