namespace Lira.Domain.Matching.Conditions.Matchers.Elapsed;

internal class ElapsedConditionMatcher : IConditionMatcher
{
    private readonly IComparableMatchFunction<TimeSpan>[] _functions;

    public ElapsedConditionMatcher(params IComparableMatchFunction<TimeSpan>[] functions)
    {
        _functions = functions;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        var firstInvokeTime = statistic.Entries.First().InvokeTime;
        var elapsed = DateTime.Now - firstInvokeTime;

        return _functions.All(f => f.IsMatch(elapsed));
    }
}
