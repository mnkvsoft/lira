namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions.Matchers.Elapsed;

internal class ElapsedConditionMatcher : IConditionMatcher
{
    private readonly IComparableMatchFunction<TimeSpan> _function;

    public ElapsedConditionMatcher(IComparableMatchFunction<TimeSpan> function)
    {
        _function = function;
    }

    public bool IsMatch(RequestStatistic statistic)
    {
        var firstInvokeTime = statistic.Entries.First().InvokeTime;
        var elapsed = DateTime.Now - firstInvokeTime;

        var result = _function.IsMatch(elapsed);
        return result;
    }
}
