using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Platform.Geography.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.Geography.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProvinceToProvinceDtoMapper : MapperBase<Province, ProvinceDto>
{
    public override partial ProvinceDto Map(Province source);
    public override partial void Map(Province source, ProvinceDto destination);
}
