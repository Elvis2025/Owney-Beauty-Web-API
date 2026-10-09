using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Platform.Geography;

/// <summary>Municipio de una provincia (PLT-M01).</summary>
public class Municipality : AuditedAggregateRoot<Guid>
{
    public Guid ProvinceId { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    protected Municipality()
    {
    }

    public Municipality(Guid id, Guid provinceId, string code, string name)
        : base(id)
    {
        SetProvince(provinceId);
        SetCode(code);
        SetName(name);
        IsActive = true;
    }

    public Municipality SetProvince(Guid provinceId)
    {
        ProvinceId = Check.NotDefaultOrNull<Guid>(provinceId, nameof(provinceId));
        return this;
    }

    public Municipality SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), PlatformConsts.MaxCodeLength).Trim();
        return this;
    }

    public Municipality SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), PlatformConsts.MaxNameLength).Trim();
        return this;
    }

    public Municipality SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }
}
