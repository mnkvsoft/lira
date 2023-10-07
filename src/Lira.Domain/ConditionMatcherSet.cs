using ArgValidation;
using Lira.Domain.Matching.Conditions;

namespace Lira.Domain;

public class ConditionMatcherSet
{
    private readonly IReadOnlyCollection<IConditionMatcher> _conditionMatchers;
    private readonly IRequestStatisticStorage _requestStatisticStorage;

    public ConditionMatcherSet(IRequestStatisticStorage requestStatisticStorage, IReadOnlyCollection<IConditionMatcher> conditionMatchers)
    {
        Arg.NotEmpty(conditionMatchers, nameof(conditionMatchers));

        _requestStatisticStorage = requestStatisticStorage;
        _conditionMatchers = conditionMatchers;
    }

    public async Task<bool> IsMatch(RequestData request, Guid requestId)
    {
        var statistic = await _requestStatisticStorage.Add(request, requestId);

        var isMatch = true;

        foreach (var conditionMatcher in _conditionMatchers)
        {
            if (!conditionMatcher.IsMatch(statistic))
            {
                isMatch = false;
                break;
            }
        }

        return isMatch;
    }
}
