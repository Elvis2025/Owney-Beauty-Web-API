using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Platform.Currencies.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Currencies.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CurrencyToCurrencyDtoMapper : MapperBase<Currency, CurrencyDto>
{
    public override partial CurrencyDto Map(Currency source);
    public override partial void Map(Currency source, CurrencyDto destination);
}
