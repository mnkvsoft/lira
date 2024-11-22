
using System.Collections.Immutable;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.Matching.Conditions;

internal abstract class ConditionMatcher : IRequestMatcher
{
    private readonly IRequestStatisticStorage _requestStatisticStorage;

    protected abstract bool IsMatch(RequestStatistic statistic);
    protected abstract string ConditionName { get; }

    protected ConditionMatcher(IRequestStatisticStorage requestStatisticStorage)
    {
        _requestStatisticStorage = requestStatisticStorage;
    }

    public async Task<RequestMatchResult> IsMatch(RuleExecutingContext context)
    {
        var statistic = await _requestStatisticStorage.Add(context.RequestContext);
        bool isMatch = IsMatch(statistic);

        if (!isMatch)
            return RequestMatchResult.NotMatched;

        return RequestMatchResult.Matched(name: $"condition.{ConditionName}", WeightValue.Condition);
    }
}