using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Service;

public interface ITaxRateAppService : IApplicationService
{
    Task<PagedResultDto<TaxRateDto>> GetListAsync(GetTaxRatesInput input);
    Task<TaxRateDto> GetAsync(Guid id);
    Task<TaxRateDto> CreateAsync(CreateTaxRateDto input);
    Task<TaxRateDto> UpdateAsync(Guid id, UpdateTaxRateDto input);

    /// <summary>Tasa vigente para un país y código en un instante (UTC). Null si no hay ninguna vigente.</summary>
    Task<TaxRateDto?> GetEffectiveAsync(string countryCode, string code, DateTime? at = null);
}
