using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Guid.Ranges;

class GuidSetIntervalDataRange : GuidDataRange
{
    public Int64Interval Interval { get; }

    public GuidSetIntervalDataRange(DataName name, Int64Interval interval) : base(name)
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
