using Lira.Domain.Actions;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public class Rule
{
    public string Name { get; }

    private readonly IReadOnlyCollection<IRequestMatcher> _matchers;
    private readonly ActionsExecutor _actionsExecutor;
    private readonly ResponseStrategy _responseStrategy;

    public Rule(
        string name,
        IReadOnlyCollection<IRequestMatcher> matchers,
        ActionsExecutor actionsExecutor,
        ResponseStrategy responseStrategy)
    {

        Name = name;
        _actionsExecutor = actionsExecutor;
        _responseStrategy = responseStrategy;
        _matchers = matchers;
    }

    public async Task<IRuleExecutor?> GetExecutor(RequestContext context)
    {
        var ruleExecutingContext = new RuleExecutingContext(context);
        var matchResult = await IsMatch(ruleExecutingContext);

        if (matchResult is RuleMatchResult.Matched matched)
            return new RuleExecutor(ruleExecutingContext, this, matched.Weight); ;

        return null;
    }

    private async Task Execute(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestContext.RequestData.SaveBody();

        await _actionsExecutor.Execute(httpContextData.RuleExecutingContext);
        await _responseStrategy.Execute(httpContextData);
    }

    private async Task<RuleMatchResult> IsMatch(RuleExecutingContext context)
    {
        var matcheds = new List<Matched>();

        foreach (var matcher in _matchers)
        {
            var matchResult = await matcher.IsMatch(context);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matcheds.Add(matched);
        }

        return new RuleMatchResult.Matched(new RuleMatchWeight(matcheds));
    }

    record RuleExecutor(RuleExecutingContext RuleExecutingContext, Rule Rule, IRuleMatchWeight Weight) : IRuleExecutor
    {
        public Task Execute(HttpResponse response)
        {
            return Rule.Execute(new HttpContextData(RuleExecutingContext, response));
        }
    }
}