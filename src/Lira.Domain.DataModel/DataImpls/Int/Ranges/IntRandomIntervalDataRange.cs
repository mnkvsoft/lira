using Lira.Common;
using Lira.Common.Extensions;
using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Int.Ranges;

public class IntRandomIntervalDataRange : IntDataRange
{
    private Interval<long> Interval { get; }

    public IntRandomIntervalDataRange(DataName name, Interval<long> interval) : base(name)
    {
        Interval = interval;
    }

    public override long Next() => Random.Shared.NextInt64(Interval);

    public override bool IsBelong(long value)
    {
        return Interval.InRange(value);
    }

    public override IStateful? GetStateful() => null;
}
