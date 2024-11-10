namespace Lira.Common;

public class Int64Sequence
{
    public Interval<long> Interval { get; }
    public long Value => _value;

    private long _value;
    private bool _sealed;

    public Int64Sequence(Interval<long> interval)
    {
        Interval = interval;
        _value = Interval.From - 1;
    }

    public Int64Sequence()
    {
        Interval = new Interval<long>(1, long.MaxValue);
        _value = Interval.From - 1;
    }

    public long Next()
    {
        if (_sealed)
            throw new InvalidOperationException("Sequence already sealed");

        var value = Interlocked.Increment(ref _value);

        if (value >= Interval.To)
            throw new Exception($"Maximum value '{Interval.To}' reached in sequence");

        return value;
    }

    public void SetValue(long value)
    {
        _value = value;
    }

    public void Seal()
    {
        _sealed = true;
    }
}
