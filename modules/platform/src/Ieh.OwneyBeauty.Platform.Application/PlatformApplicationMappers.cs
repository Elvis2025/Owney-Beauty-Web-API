using Ieh.OwneyBeauty.Platform.Auditing;
using Ieh.OwneyBeauty.Platform.Currencies;
using Ieh.OwneyBeauty.Platform.Geography;
using Ieh.OwneyBeauty.Platform.Taxes;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace Ieh.OwneyBeauty.Platform;

/* Solo mapeos Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio,
 * nunca mapeando un DTO sobre la entidad (protege las invariantes).
 */

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProvinceToProvinceDtoMapper : MapperBase<Province, ProvinceDto>
{
    public override partial ProvinceDto Map(Province source);
    public override partial void Map(Province source, ProvinceDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MunicipalityToMunicipalityDtoMapper : MapperBase<Municipality, MunicipalityDto>
{
    public override partial MunicipalityDto Map(Municipality source);
    public override partial void Map(Municipality source, MunicipalityDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CurrencyToCurrencyDtoMapper : MapperBase<Currency, CurrencyDto>
{
    public override partial CurrencyDto Map(Currency source);
    public override partial void Map(Currency source, CurrencyDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TaxRateToTaxRateDtoMapper : MapperBase<TaxRate, TaxRateDto>
{
    public override partial TaxRateDto Map(TaxRate source);
    public override partial void Map(TaxRate source, TaxRateDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BusinessAuditEventToDtoMapper : MapperBase<BusinessAuditEvent, BusinessAuditEventDto>
{
    public override partial BusinessAuditEventDto Map(BusinessAuditEvent source);
    public override partial void Map(BusinessAuditEvent source, BusinessAuditEventDto destination);
}
