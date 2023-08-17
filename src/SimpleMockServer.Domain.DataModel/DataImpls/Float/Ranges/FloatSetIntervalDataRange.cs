using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Float.Ranges;

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
        decimal range = Interval.To - Interval.From;
        double sample = Random.Shared.NextDouble();
        decimal scaled = (decimal)sample * range + Interval.From;
        scaled = Math.Round(scaled, _decimals);
        return scaled;
    }

    public override bool IsBelong(decimal value)
    {
        return Interval.InRange(value);
    }
}
