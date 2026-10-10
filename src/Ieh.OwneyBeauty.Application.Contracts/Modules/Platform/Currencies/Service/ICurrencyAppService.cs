using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Platform.Currencies.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.Currencies.Service;

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
