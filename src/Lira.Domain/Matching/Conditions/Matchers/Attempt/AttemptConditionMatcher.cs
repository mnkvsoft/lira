namespace Lira.Domain.Matching.Conditions.Matchers.Attempt;
internal class AttemptConditionMatcher : IConditionMatcher
{
    private readonly IComparableMatchFunction<int>[] _functions;

    public AttemptConditionMatcher(params IComparableMatchFunction<int>[] functions)
    {
        _functions = functions;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        var attempt = statistic.Entries.Count;
        return _functions.All(f => f.IsMatch(attempt));
    }
}
