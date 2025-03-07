namespace Lira.Common.Extensions;

public static class RandomExtensions
{
    public static int Next(this Random random, Interval<int> interval)
    {
        return random.Next(interval.From, interval.To);
    }
    public static long NextInt64(this Random random, Interval<long> interval)
    {
        return random.NextInt64(interval.From, interval.To);
    }

    public static decimal NextDecimal(this Random random, Interval<decimal> interval)
    {
        decimal range = interval.To - interval.From;
        double sample = random.NextDouble();
        decimal scaled = (decimal)sample * range + interval.From;
        return scaled;
    }
}
