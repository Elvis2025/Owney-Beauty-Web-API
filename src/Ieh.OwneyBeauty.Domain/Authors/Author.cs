using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Ieh.OwneyBeauty.Authors;

public class Author : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; }

    public DateTime BirthDate { get; set; }

    public string? ShortBio { get; set; }
}
