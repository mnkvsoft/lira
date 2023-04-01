

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Attempt;

internal class AttemptConditionMatcher : IConditionMatcher
{
    private readonly IIntMatchFunction _attemptMatchFunction;

    public AttemptConditionMatcher(IIntMatchFunction attemptMatchFunction)
    {
        _attemptMatchFunction = attemptMatchFunction;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        int attempt = statistic.Entries.Count;
        return _attemptMatchFunction.IsMatch(attempt);
    }
}
