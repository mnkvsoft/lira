using Lira.Domain.Actions;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public class Rule
{
    public string Name { get; }

    private readonly RequestMatcherSet _requestMatcherSet;
    private readonly ActionsExecutor? _actionsExecutor;
    private readonly ResponseStrategy _responseStrategy;

    public Rule(
        string name,
        RequestMatcherSet matchers,
        ActionsExecutor? actionsExecutor,
        ResponseStrategy responseStrategy)
    {
        _requestMatcherSet = matchers;

        Name = name;
        _actionsExecutor = actionsExecutor;
        _responseStrategy = responseStrategy;
    }

    public async Task<IRuleExecutor?> GetExecutor(RequestContext context)
    {
        var matchResult = await _requestMatcherSet.IsMatch(context);

        if (matchResult is RuleMatchResult.Matched matched)
            return new RuleExecutor(context.RequestData, Rule: this, matched.MatchedValues, matched.Weight);

        return null;
    }

    private async Task Execute(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.Request.SaveBody();

        if (_actionsExecutor != null)
        {
            await _actionsExecutor.Execute(httpContextData.RuleExecutingContext);
        }

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