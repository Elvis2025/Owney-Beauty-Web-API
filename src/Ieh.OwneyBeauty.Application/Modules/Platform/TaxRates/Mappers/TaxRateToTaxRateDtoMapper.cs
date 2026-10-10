using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Platform.Models;
using Ieh.OwneyBeauty.Modules.Platform.TaxRates.Dto;

namespace Ieh.OwneyBeauty.Modules.Platform.TaxRates.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TaxRateToTaxRateDtoMapper : MapperBase<TaxRate, TaxRateDto>
{
    public override partial TaxRateDto Map(TaxRate source);
    public override partial void Map(TaxRate source, TaxRateDto destination);
}
