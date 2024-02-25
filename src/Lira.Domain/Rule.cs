using Lira.Domain.Actions;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public class Rule
{
    public string Name { get; }

    private readonly RequestMatcherSet _requestMatcherSet;
    private readonly ConditionMatcherSet? _conditionMatcherSet;
    private readonly ActionsExecutor _actionsExecutor;
    private readonly ResponseStrategy _responseStrategy;

    public Rule(
        string name,
        RequestMatcherSet matchers,
        ConditionMatcherSet? conditionMatcherSet,
        ActionsExecutor actionsExecutor,
        ResponseStrategy responseStrategy)
    {
        _requestMatcherSet = matchers;

        Name = name;
        _conditionMatcherSet = conditionMatcherSet;
        _actionsExecutor = actionsExecutor;
        _responseStrategy = responseStrategy;
    }

    public async Task<IRuleExecutor?> GetExecutor(RequestData request, Guid requestId)
    {
        var matchResult = await _requestMatcherSet.IsMatch(request);

        if (matchResult is RuleMatchResult.NotMatched)
            return null;

        var matched = (RuleMatchResult.Matched)matchResult;
        if (_conditionMatcherSet == null)
            return new RuleExecutor(request, this, matched.MatchedValues, matched.Weight);

        bool conditionsIsMatch = await _conditionMatcherSet.IsMatch(request, requestId);

        if (conditionsIsMatch)
            return new RuleExecutor(request, this, matched.MatchedValues, matched.Weight); ;

        return null;
    }

    internal async Task Execute(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.Request.SaveBody();

        await _actionsExecutor.Execute(httpContextData.RuleExecutingContext);
        await _responseStrategy.Execute(httpContextData);
    }

    record RuleExecutor(RequestData Request, Rule Rule, IReadOnlyDictionary<string, string?> MatchedValues, IRuleMatchWeight Weight) : IRuleExecutor
    {
        public Task Execute(HttpResponse response)
        {
            return Rule.Execute(new HttpContextData(new RuleExecutingContext(Request, MatchedValues), response));
        }
    }
}