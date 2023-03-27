namespace SimpleMockServer.Common;

public record Interval<T> where T : struct, IComparable<T>
{
    public T From { get; }
    public T To { get; }

    public Interval(T from, T to)
    {
        if (from.CompareTo(to) >= 0)
            throw new ArgumentOutOfRangeException($"Argument 'from' ({from}) must be less or equal than argument 'to' ({to})");

        From = from;
        To = to;
    }

    public override string ToString() => $"[{From} - {To}]";
    public bool InRange(T value) => (From.CompareTo(value) <= 0) && (value.CompareTo(To) <= 0);
    public bool IsIntersect(Interval<T> interval)
    {
        return interval.From.CompareTo(To) >= 0 || From.CompareTo(interval.To) >= 0;
    }
}


public record Int64Interval(Int64 from, Int64 to) : Interval<Int64>(from, to);