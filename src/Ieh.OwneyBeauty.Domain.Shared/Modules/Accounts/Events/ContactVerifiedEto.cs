using System;
using Volo.Abp.EventBus;
using Ieh.OwneyBeauty.Modules.Accounts.Enums;

namespace Ieh.OwneyBeauty.Modules.Accounts.Events;

[EventName("Owney.Accounts.ContactVerified")]
public class ContactVerifiedEto
{
    public Guid UserId { get; set; }
    public VerificationChannel Channel { get; set; }
}
