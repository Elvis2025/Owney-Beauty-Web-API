using System;
using Volo.Abp.EventBus;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Events;

/// <summary>Evento de dominio "UserRegistered" (§14.3). Se publica por outbox.</summary>
[EventName("Owney.Accounts.UserRegistered")]
public class UserRegisteredEto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public AccountMode InitialMode { get; set; }
    public DateTime RegisteredAt { get; set; }
}
