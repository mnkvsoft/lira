using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;

public class GuidSetIntervalDataRange : GuidDataRange
{
    public Interval<long> Interval { get; }

    public GuidSetIntervalDataRange(DataName name, Interval<long> interval) : base(name)
    {
        Interval = interval;
    }

    public override System.Guid Next()
    {
        return Random.Shared.NextInt64(Interval.From, Interval.To).ToGuid();
    }

    public override bool IsBelong(System.Guid value)
    {
        return Interval.InRange(value.ToLong());
    }
}
