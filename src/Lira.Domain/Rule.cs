using Lira.Common;
using Lira.Domain.Handling;

namespace Lira.Domain;
//
// internal record RuleData
// {
//     public string Info { get; }
//     public IReadOnlyCollection<IRequestMatcher> Matchers { get; }
//     public IReadOnlyCollection<Factory<Delayed<Middleware>>> MiddlewaresFactories { get; }
//
//     public RuleData(string info,
//         IReadOnlyCollection<IRequestMatcher> matchers,
//         IReadOnlyCollection<Factory<Delayed<Middleware>>> middlewaresFactories)
//     {
//         Info = info;
//         Matchers = matchers;
//         MiddlewaresFactories = middlewaresFactories;
//     }
// }

internal class Rule
{
    public string Info { get; }
    private readonly IReadOnlyCollection<IRequestMatcher> _matchers;
    private readonly IReadOnlyCollection<Factory<Delayed<Middleware>>> _middlewaresFactories;

    public Rule(
        string info,
        IReadOnlyCollection<IRequestMatcher> matchers,
        IReadOnlyCollection<Factory<Delayed<Middleware>>> middlewaresFactories)
    {
        Info = info;
        _matchers = matchers;
        _middlewaresFactories = middlewaresFactories;
    }


    public async Task Handle(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestData.SaveBody();

        foreach (var factory in _middlewaresFactories)
        {
            var delayedMiddleware = factory();

            if(delayedMiddleware.GetDelay != null)
                await Task.Delay(delayedMiddleware.GetDelay(httpContextData.RuleExecutingContext));

            var middleware = delayedMiddleware.Value;
            await middleware.Handle(httpContextData);
        }
    }

    public async Task<RuleMatchResult> IsMatch(RuleExecutingContext context)
    {
        var matcheds = new List<Matched>();

        foreach (var matcher in _matchers)
        {
            var matchResult = await matcher.IsMatch(context);
            if (matchResult is not Matched matched)
                return RuleMatchResult.NotMatched.Instance;

            matcheds.Add(matched);
        }

        return new RuleMatchResult.Matched(new RuleMatchWeight(matcheds), this);
    }
}