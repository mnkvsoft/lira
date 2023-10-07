using Lira.Common;

namespace Lira.Domain.DataModel.DataImpls.Float.Ranges;

public class FloatSetIntervalDataRange : FloatDataRange
{
    private readonly int _decimals;
    private Interval<decimal> Interval { get; }

    public FloatSetIntervalDataRange(DataName name, Interval<decimal> interval, int decimals) : base(name)
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
