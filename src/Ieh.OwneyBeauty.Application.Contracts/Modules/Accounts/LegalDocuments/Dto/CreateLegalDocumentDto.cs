using System.ComponentModel.DataAnnotations;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

public class CreateLegalDocumentDto
{
    public LegalDocumentType Type { get; set; }

    [Required, StringLength(AccountsConsts.MaxLanguageLength)]
    public string Language { get; set; } = "es";

    [Required, StringLength(AccountsConsts.MaxLegalVersionLength)]
    public string Version { get; set; } = null!;

    [Required, StringLength(AccountsConsts.MaxLegalTitleLength)]
    public string Title { get; set; } = null!;

    [StringLength(AccountsConsts.MaxLegalContentLength)]
    public string? Content { get; set; }

    [Url, StringLength(AccountsConsts.MaxLegalUrlLength)]
    public string? ExternalUrl { get; set; }

    public bool RequiresAcceptance { get; set; } = true;
}
