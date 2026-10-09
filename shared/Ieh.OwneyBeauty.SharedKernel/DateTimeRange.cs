using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace Ieh.OwneyBeauty.SharedKernel;

/// <summary>Intervalo semiabierto [Start, End) en UTC. Base de agenda, reservas y vigencias.</summary>
public sealed class DateTimeRange : ValueObject
{
    public DateTime Start { get; private set; }

    public DateTime End { get; private set; }

    private DateTimeRange()
    {
    }

    public DateTimeRange(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException(OwneyErrorCodes.Shared.DateTimeMustBeUtc);
        }

        if (end <= start)
        {
            throw new BusinessException(OwneyErrorCodes.Shared.InvalidDateRange)
                .WithData("start", start)
                .WithData("end", end);
        }

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    public bool Contains(DateTime instant) => instant >= Start && instant < End;

    public bool Overlaps(DateTimeRange other)
    {
        Check.NotNull(other, nameof(other));
        return Start < other.End && other.Start < End;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Start;
        yield return End;
    }
}
