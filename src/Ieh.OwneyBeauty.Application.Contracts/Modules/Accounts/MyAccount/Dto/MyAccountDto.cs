using System;
using System.Collections.Generic;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;
using Ieh.OwneyBeauty.Modules.Accounts.LegalDocuments.Dto;

namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

public class MyAccountDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<AccountMode> AvailableModes { get; set; } = new();
    public AccountMode ActiveMode { get; set; }
    public OnboardingStep OnboardingStep { get; set; }
    public List<PublicLegalDocumentDto> PendingLegalDocuments { get; set; } = new();
}
