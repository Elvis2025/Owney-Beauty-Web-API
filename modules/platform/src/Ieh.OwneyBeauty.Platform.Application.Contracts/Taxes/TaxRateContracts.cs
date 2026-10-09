using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty.Platform.Taxes;

public class TaxRateDto : AuditedEntityDto<Guid>
{
    public string CountryCode { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTaxRateDto
{
    [Required, StringLength(PlatformConsts.CountryCodeLength, MinimumLength = PlatformConsts.CountryCodeLength)]
    public string CountryCode { get; set; } = PlatformConsts.DefaultMarket;

    [Required, StringLength(PlatformConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    /// <summary>Fracción: 0.18 = 18 %.</summary>
    [Range(typeof(decimal), "0", "1")]
    public decimal Rate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }
}

public class UpdateTaxRateDto
{
    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Range(typeof(decimal), "0", "1")]
    public decimal Rate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}

public class GetTaxRatesInput : PagedAndSortedResultRequestDto
{
    public string? CountryCode { get; set; }
    public string? Code { get; set; }
}

public interface ITaxRateAppService : IApplicationService
{
    Task<PagedResultDto<TaxRateDto>> GetListAsync(GetTaxRatesInput input);
    Task<TaxRateDto> GetAsync(Guid id);
    Task<TaxRateDto> CreateAsync(CreateTaxRateDto input);
    Task<TaxRateDto> UpdateAsync(Guid id, UpdateTaxRateDto input);

    /// <summary>Tasa vigente para un país y código en un instante (UTC). Null si no hay ninguna vigente.</summary>
    Task<TaxRateDto?> GetEffectiveAsync(string countryCode, string code, DateTime? at = null);
}
