using System;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Registration.Dto;

public class RegistrationResultDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public AccountMode ActiveMode { get; set; }
    public bool RequiresEmailVerification { get; set; }
    public bool RequiresPhoneVerification { get; set; }
}
