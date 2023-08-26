using System.Diagnostics;

namespace SimpleMockServer.Common;

public static class StopwatchExtensions
{
    public static double GetElapsedDoubleMilliseconds(this Stopwatch sw)
    {
        double ticks = sw.ElapsedTicks;
        return Math.Round((ticks / Stopwatch.Frequency) * 1000, 2);
    }
}