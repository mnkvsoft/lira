using ArgValidation;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;

namespace SimpleMockServer.Domain.Models.RulesModel;

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
        await _requestStatisticStorage.Add(request, requestId);

        bool isMatch = true;

        RequestStatistic? statistic = await _requestStatisticStorage.Get(request);

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
