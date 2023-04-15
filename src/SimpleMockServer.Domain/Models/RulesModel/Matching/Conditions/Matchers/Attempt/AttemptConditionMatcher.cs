

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;

internal class AttemptConditionMatcher : IConditionMatcher
{
    private readonly IComparableMatchFunction<int> _attemptMatchFunction;

    public AttemptConditionMatcher(IComparableMatchFunction<int> attemptMatchFunction)
    {
        _attemptMatchFunction = attemptMatchFunction;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        int attempt = statistic.Entries.Count;
        return _attemptMatchFunction.IsMatch(attempt);
    }
}
