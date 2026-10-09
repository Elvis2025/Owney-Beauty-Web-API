using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ieh.OwneyBeauty.Platform.Currencies;

public class CurrencyDto : AuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public int DecimalPlaces { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCurrencyDto
{
    [Required, StringLength(PlatformConsts.CurrencyCodeLength, MinimumLength = PlatformConsts.CurrencyCodeLength)]
    public string Code { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxCurrencySymbolLength)]
    public string Symbol { get; set; } = null!;

    [Range(0, 4)]
    public int DecimalPlaces { get; set; } = 2;
}

public class UpdateCurrencyDto
{
    [Required, StringLength(PlatformConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required, StringLength(PlatformConsts.MaxCurrencySymbolLength)]
    public string Symbol { get; set; } = null!;

    [Range(0, 4)]
    public int DecimalPlaces { get; set; } = 2;

    public bool IsActive { get; set; } = true;
}

public interface ICurrencyAppService : IApplicationService
{
    /// <summary>Público: monedas activas.</summary>
    Task<ListResultDto<CurrencyDto>> GetActiveAsync();

    Task<ListResultDto<CurrencyDto>> GetListAsync();
    Task<CurrencyDto> GetAsync(Guid id);
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto input);
    Task<CurrencyDto> UpdateAsync(Guid id, UpdateCurrencyDto input);
    Task<CurrencyDto> SetDefaultAsync(Guid id);
}
