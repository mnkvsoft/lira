using Lira.Common;
using Lira.Domain.Handling;

namespace Lira.Domain;

public record RuleMiddlewares(
    IReadOnlyCollection<Delayed<IAction>> PreResponseActionFactories,
    Factory<Delayed<IResponseGenerator>> ResponseGeneratorFactory,
    IReadOnlyCollection<Delayed<IAction>> PostResponseActionFactories
);

internal class Rule
{
    public string Info { get; }
    private readonly IReadOnlyCollection<IRequestMatcher> _matchers;

    private readonly IReadOnlyCollection<Delayed<IAction>> _preResponseActions;
    private readonly Factory<Delayed<IResponseGenerator>> _responseGeneratorFactory;
    private readonly IReadOnlyCollection<Delayed<IAction>> _postResponseActions;

    public Rule(
        string info,
        IReadOnlyCollection<IRequestMatcher> matchers,
        RuleMiddlewares middlewares)
    {
        Info = info;
        _matchers = matchers;
        _preResponseActions = middlewares.PreResponseActionFactories;
        _responseGeneratorFactory = middlewares.ResponseGeneratorFactory;
        _postResponseActions = middlewares.PostResponseActionFactories;
    }


    public async Task Handle(HttpContextData httpContextData)
    {
        await httpContextData.RuleExecutingContext.RequestData.SaveBody();

        foreach (var delayedAction in _preResponseActions)
        {
            if(delayedAction.GetDelay != null)
                await Task.Delay(delayedAction.GetDelay(httpContextData.RuleExecutingContext));

            var action = delayedAction.Value;
            await action.Execute(httpContextData.RuleExecutingContext);
        }

        var delayedResponse = _responseGeneratorFactory();
        if(delayedResponse.GetDelay != null)
            await Task.Delay(delayedResponse.GetDelay(httpContextData.RuleExecutingContext));
        var responseHandler = delayedResponse.Value;
        await responseHandler.Generate(httpContextData);

        if (_postResponseActions.Count > 0)
        {
            _ = Task.Run(async () =>
            {
                foreach (var delayedAction in _postResponseActions)
                {
                    if (delayedAction.GetDelay != null)
                        await Task.Delay(delayedAction.GetDelay(httpContextData.RuleExecutingContext));

                    var action = delayedAction.Value;
                    await action.Execute(httpContextData.RuleExecutingContext);
                }
            });
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