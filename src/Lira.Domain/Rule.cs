using Lira.Domain.Actions;

namespace Lira.Domain;

public class Rule
{
    public string Name { get; }

    private readonly RequestMatcherSet _requestMatcherSet;
    private readonly ConditionMatcherSet? _conditionMatcherSet;
    private readonly ActionsExecutor _actionsExecutor;
    private readonly ResponseStrategy _responseStrategy;
    public IReadOnlyCollection<PathNameMap> PathNameMaps { get; }

    public Rule(
        string name,
        RequestMatcherSet matchers,
        ConditionMatcherSet? conditionMatcherSet,
        ActionsExecutor actionsExecutor,
        IReadOnlyCollection<PathNameMap> pathNameMaps, 
        ResponseStrategy responseStrategy)
    {
        _requestMatcherSet = matchers;

        Name = name;
        _conditionMatcherSet = conditionMatcherSet;
        _actionsExecutor = actionsExecutor;
        PathNameMaps = pathNameMaps;
        _responseStrategy = responseStrategy;
    }

    public async Task<RuleMatchResult> IsMatch(RequestData request, Guid requestId)
    {
        var matchResult = await _requestMatcherSet.IsMatch(request);

        if (matchResult is RuleMatchResult.NotMatched notMatched)
            return notMatched;

        var matched = (RuleMatchResult.Matched)matchResult;
        if (_conditionMatcherSet == null)
            return matched;

        bool conditionsIsMatch = await _conditionMatcherSet.IsMatch(request, requestId);

        if (conditionsIsMatch)
            return matched;

        return RuleMatchResult.NotMatchedInstance;
    }

    public async Task Execute(HttpContextData httpContextData)
    {
        await httpContextData.Request.SaveBody();

        await _actionsExecutor.Execute(httpContextData.Request);
        await _responseStrategy.Execute(httpContextData);
    }
}