using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Dto;
using Ieh.OwneyBeauty.Modules.Platform.Models;

namespace Ieh.OwneyBeauty.Modules.Platform.BusinessAudit.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BusinessAuditEventToDtoMapper : MapperBase<BusinessAuditEvent, BusinessAuditEventDto>
{
    public override partial BusinessAuditEventDto Map(BusinessAuditEvent source);
    public override partial void Map(BusinessAuditEvent source, BusinessAuditEventDto destination);
}
