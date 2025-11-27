using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.DataModel.DataImpls.Dec.Ranges;

public class DecSetIntervalDataRange : DecDataRange
{
    private readonly int _decimals;
    private Interval<decimal> Interval { get; }

    public DecSetIntervalDataRange(DataName name, Interval<decimal> interval, int decimals, string? description) : base(name, description)
    {
        _decimals = decimals;
        Interval = interval;
    }

    public override decimal Next()
    {
        decimal scaled = Random.Shared.NextDecimal(Interval);
        scaled = Math.Round(scaled, _decimals);
        return scaled;
    }

    public override bool IsBelong(decimal value)
    {
        return Interval.InRange(value);
    }
}
