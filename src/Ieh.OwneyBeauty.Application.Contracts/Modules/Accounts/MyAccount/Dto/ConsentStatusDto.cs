using System;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.MyAccount.Dto;

public class ConsentStatusDto
{
    public ConsentType Type { get; set; }
    public bool Granted { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
