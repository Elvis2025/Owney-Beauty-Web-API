using System;
using Volo.Abp.EventBus;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Events;

[EventName("Owney.Accounts.AccountModeChanged")]
public class AccountModeChangedEto
{
    public Guid UserId { get; set; }
    public AccountMode PreviousMode { get; set; }
    public AccountMode NewMode { get; set; }
}
