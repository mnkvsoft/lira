using Lira.Common.Exceptions;
using Lira.Domain.Handling;

namespace Lira.Domain;

public record RuleData
{
    public string Info { get; }
    public IReadOnlyCollection<IRequestMatcher> Matchers { get; }
    public IReadOnlyCollection<Func<Delayed<IMiddleware>>> MiddlewareFactories { get; }

    public RuleData(string info,
        IReadOnlyCollection<IRequestMatcher> matchers,
        IReadOnlyCollection<Func<Delayed<IMiddleware>>> middlewareFactories)
    {
        Info = info;
        Matchers = matchers;
        MiddlewareFactories = middlewareFactories;
    }
}

internal class Rule(RuleData data)
{
    public string Info { get; } = data.Info;

    public async Task Handle(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestData.SaveBody();

        bool wasHandled = false;
        foreach (var factory in data.MiddlewareFactories)
        {
            var middleware = factory();

            if(middleware.GetDelay != null)
                await Task.Delay(middleware.GetDelay(httpContextData.RuleExecutingContext));

            if (middleware.Value is IHandler handler)
            {
                wasHandled = true;
                await handler.Handle(httpContextData);
            }
            else if (middleware.Value is IAction action)
            {
                await action.Execute(httpContextData.RuleExecutingContext);
            }
            else
            {
                throw new UnsupportedInstanceType(factory);
            }
        }

        // if (!wasHandled)
        //     throw new Exception("No handler was found");
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