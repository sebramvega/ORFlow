namespace ORFlow.Domain.Scheduling;

public class TimeRange
{
    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End time must be later than start time.");
        }

        Start = start;
        End = end;
        
    }

    public bool OverlapsWith(TimeRange other)
    {
        // Back-to-back ranges are allowed; touching endpoints do not count as overlap.
        return Start < other.End && other.Start < End;
    }

    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }
}