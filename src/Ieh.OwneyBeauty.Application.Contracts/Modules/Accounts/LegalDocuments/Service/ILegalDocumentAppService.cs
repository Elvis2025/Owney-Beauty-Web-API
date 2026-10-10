using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Service;

public interface ILegalDocumentAppService : IApplicationService
{
    /// <summary>Público: documentos vigentes en un idioma (con respaldo en español).</summary>
    Task<ListResultDto<PublicLegalDocumentDto>> GetCurrentAsync(string language = "es");

    Task<PagedResultDto<LegalDocumentDto>> GetListAsync(GetLegalDocumentsInput input);
    Task<LegalDocumentDto> GetAsync(Guid id);
    Task<LegalDocumentDto> CreateAsync(CreateLegalDocumentDto input);
    Task<LegalDocumentDto> UpdateAsync(Guid id, UpdateLegalDocumentDto input);
    Task<LegalDocumentDto> PublishAsync(Guid id);
}
