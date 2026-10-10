using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

public class UpdateLegalDocumentDto
{
    [Required, StringLength(AccountsConsts.MaxLegalTitleLength)]
    public string Title { get; set; } = null!;

    [StringLength(AccountsConsts.MaxLegalContentLength)]
    public string? Content { get; set; }

    [Url, StringLength(AccountsConsts.MaxLegalUrlLength)]
    public string? ExternalUrl { get; set; }

    public bool RequiresAcceptance { get; set; } = true;
}
