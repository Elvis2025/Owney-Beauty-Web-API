using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;
using Ieh.OwneyBeauty.Modules.Accounts.Models;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Mappers;

/* Solo Entidad → DTO. Las entidades se crean/modifican con sus métodos de dominio. */
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LegalDocumentToLegalDocumentDtoMapper : MapperBase<LegalDocument, LegalDocumentDto>
{
    public override partial LegalDocumentDto Map(LegalDocument source);
    public override partial void Map(LegalDocument source, LegalDocumentDto destination);
}
