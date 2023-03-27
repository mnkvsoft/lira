using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.Models.DataModel;

class Int64Sequence
{
    public Interval<long> Interval { get; }
    private long _counter;

    public Int64Sequence(Interval<long> interval)
    {
        Interval = interval;
        _counter = interval.From - 1;
    }

    public long Next()
    {
        long value = Interlocked.Increment(ref _counter);

        if (value >= Interval.To)
            throw new Exception($"Maximum value '{Interval.To}' reached in sequence");

        return value;
    }
}