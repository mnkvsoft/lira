namespace Lira.Domain;

public record RuleData(
    string Info,
    IReadOnlyCollection<IRequestMatcher> Matchers,
    IReadOnlyCollection<Delayed<IHandler>> Handlers);

internal class Rule(RuleData data)
{
    public string Info { get; } = data.Info;

    public async Task Handle(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestData.SaveBody();

        foreach (var handler in data.Handlers)
        {
            if(handler.GetDelay != null)
                await Task.Delay(handler.GetDelay(httpContextData.RuleExecutingContext));

            await handler.Value.Handle(httpContextData);
        }
    }

    public async Task<RuleMatchResult> IsMatch(RuleExecutingContext context)
    {
        var matcheds = new List<Matched>();

        foreach (var matcher in data.Matchers)
        {
            var matchResult = await matcher.IsMatch(context);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matcheds.Add(matched);
        }

        return new RuleMatchResult.Matched(new RuleMatchWeight(matcheds), this);
    }
}