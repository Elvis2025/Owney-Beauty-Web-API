using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

public class AcceptLegalDocumentsDto
{
    [Required, MinLength(1)]
    public List<Guid> LegalDocumentIds { get; set; } = new();
}
