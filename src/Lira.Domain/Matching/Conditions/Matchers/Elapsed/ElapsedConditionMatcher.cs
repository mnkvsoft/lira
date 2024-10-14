namespace Lira.Domain.Matching.Conditions.Matchers.Elapsed;

internal class ElapsedConditionMatcher : ConditionMatcher
{
    private readonly IComparableMatchFunction<TimeSpan>[] _functions;

    public ElapsedConditionMatcher(IRequestStatisticStorage requestStatisticStorage, params IComparableMatchFunction<TimeSpan>[] functions) : base(requestStatisticStorage)
    {
        _functions = functions;
    }

    protected override string ConditionName => "elapsed";

    protected override bool IsMatch(RequestStatistic statistic)
    {
        var firstInvokeTime = statistic.Entries.Min(x => x.InvokeTime);
        var elapsed = DateTime.Now - firstInvokeTime;

        return _functions.All(f => f.IsMatch(elapsed));
    }
}
