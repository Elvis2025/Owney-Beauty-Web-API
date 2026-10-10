using System;
using Volo.Abp.Application.Dtos;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

public class LegalDocumentDto : FullAuditedEntityDto<Guid>
{
    public LegalDocumentType Type { get; set; }
    public string Language { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? ExternalUrl { get; set; }
    public bool RequiresAcceptance { get; set; }
    public DateTime? PublishedTime { get; set; }
    public bool IsCurrent { get; set; }
}
