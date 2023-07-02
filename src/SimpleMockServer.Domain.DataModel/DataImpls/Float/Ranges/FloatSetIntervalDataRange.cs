using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Float.Ranges;

public class FloatSetIntervalDataRange : FloatDataRange
{
    public Interval<float> Interval { get; }

    public FloatSetIntervalDataRange(DataName name, Interval<float> interval) : base(name)
    {
        Interval = interval;
    }

    public override float Next()
    {
        double range = (double)Interval.To - Interval.From;
        double sample = Random.Shared.NextDouble();
        double scaled = sample * range + Interval.From;
        scaled = Math.Round(scaled, 2);
        float result = (float) scaled;
        return result;
    }

    public override bool IsBelong(float value)
    {
        return Interval.InRange(value);
    }
}
