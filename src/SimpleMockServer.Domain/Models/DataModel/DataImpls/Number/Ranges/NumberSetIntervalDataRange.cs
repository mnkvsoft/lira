using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Number.Ranges;

class NumberSetIntervalDataRange : NumberDataRange
{
    public Int64Interval Interval { get; }

    public NumberSetIntervalDataRange(DataName name, Int64Interval interval) : base(name)
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
