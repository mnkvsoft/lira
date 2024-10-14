namespace Lira.Domain.Matching.Conditions.Matchers.Attempt;
internal class AttemptConditionMatcher : ConditionMatcher
{
    private readonly IComparableMatchFunction<int>[] _functions;

    public AttemptConditionMatcher(IRequestStatisticStorage requestStatisticStorage, params IComparableMatchFunction<int>[] functions) : base(requestStatisticStorage)
    {
        _functions = functions;
    }

    protected override string ConditionName => "attempt";

    protected override bool IsMatch(RequestStatistic statistic)
    {
        var attempt = statistic.Entries.Count;
        return _functions.All(f => f.IsMatch(attempt));
    }
}
