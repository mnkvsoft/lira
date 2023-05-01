namespace SimpleMockServer.Domain.Matching.Conditions.Matchers.Attempt;
internal class AttemptConditionMatcher : IConditionMatcher
{
    private readonly IComparableMatchFunction<int> _function;

    public AttemptConditionMatcher(IComparableMatchFunction<int> function)
    {
        _function = function;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        var attempt = statistic.Entries.Count;
        return _function.IsMatch(attempt);
    }
}
