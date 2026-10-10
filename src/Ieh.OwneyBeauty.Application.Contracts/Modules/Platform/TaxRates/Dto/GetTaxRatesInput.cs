using Volo.Abp.Application.Dtos;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

public class GetTaxRatesInput : PagedAndSortedResultRequestDto
{
    public string? CountryCode { get; set; }
    public string? Code { get; set; }
}
