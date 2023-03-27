namespace SimpleMockServer.Common;

public static class RandomExtensions
{
    public static long GetNext(this Random random, Interval<long> interval)
    {
        return random.NextInt64(interval.From, interval.To);
    }
}