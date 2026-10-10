using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MunicipalityToMunicipalityDtoMapper : MapperBase<Municipality, MunicipalityDto>
{
    public override partial MunicipalityDto Map(Municipality source);
    public override partial void Map(Municipality source, MunicipalityDto destination);
}
