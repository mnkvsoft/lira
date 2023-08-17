using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Int.Ranges;

public class IntSetIntervalDataRange : IntDataRange
{
    public Interval<long> Interval { get; }

    public IntSetIntervalDataRange(DataName name, Interval<long> interval) : base(name)
    {
        Interval = interval;
    }

    public override long Next()
    {
        return Random.Shared.NextInt64(Interval.From, Interval.To);
    }

    public override bool IsBelong(long value)
    {
        return Interval.InRange(value);
    }
}
