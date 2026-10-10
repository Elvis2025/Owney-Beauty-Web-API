using Volo.Abp.Application.Dtos;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

public class GetLegalDocumentsInput : PagedAndSortedResultRequestDto
{
    public LegalDocumentType? Type { get; set; }
    public string? Language { get; set; }
    public bool? IsCurrent { get; set; }
}
