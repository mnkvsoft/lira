using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public class Rule(
    string name,
    IReadOnlyCollection<IRequestMatcher> matchers,
    IReadOnlyCollection<Delayed<IHandler>> handlers)
{
    public string Name { get; } = name;

    public async Task<IRuleExecutor?> GetExecutor(RequestContext context)
    {
        var ruleExecutingContext = new RuleExecutingContext(context);
        var matchResult = await IsMatch(ruleExecutingContext);

        if (matchResult is RuleMatchResult.Matched matched)
            return new RuleExecutor(ruleExecutingContext, this, matched.Weight);

        return null;
    }

    private async Task Handle(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestContext.RequestData.SaveBody();

        foreach (var handler in handlers)
        {
            if(handler.GetDelay != null)
                await Task.Delay(await handler.GetDelay(httpContextData.RuleExecutingContext));

            await handler.Value.Handle(httpContextData);
        }
    }

    private async Task<RuleMatchResult> IsMatch(RuleExecutingContext context)
    {
        var matcheds = new List<Matched>();

        foreach (var matcher in matchers)
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
            return Rule.Handle(new HttpContextData(RuleExecutingContext, response));
        }
    }
}