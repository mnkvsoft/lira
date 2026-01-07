using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Caching;
using Lira.Domain.Handling;
using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.ResponseStrategies;

namespace Lira.Domain;

public record ResponseMiddlewareModes(CachingMode Caching, WriteHistoryMode WriteHistory);

public interface IMiddlewareFactory
{
    Factory<Delayed<Middleware>> CreateMultipleResponse(
        ResponseMiddlewareModes modes,
        IReadOnlyList<Delayed<IResponseStrategy>> delayedStrategies);

    Factory<Delayed<Middleware>> CreateResponse(
        ResponseMiddlewareModes modes,
        Delayed<IResponseStrategy> delayedStrategy);

    Factory<Delayed<Middleware>> CreateAction(Delayed<IAction> delayedAction);
}

class MiddlewareFactory(ResponseMiddlewareFactory responseMiddlewareFactory) : IMiddlewareFactory
{
    public Factory<Delayed<Middleware>> CreateMultipleResponse(
        ResponseMiddlewareModes modes,
        IReadOnlyList<Delayed<IResponseStrategy>> delayedStrategies)
    {
        return () =>
        {
            var delayed = delayedStrategies.Random();
            var middleware = new Delayed<Middleware>(
                responseMiddlewareFactory.Create(modes, () => delayed.Value),
                delayed.GetDelay);
            return middleware;
        };
    }

    public Factory<Delayed<Middleware>> CreateResponse(ResponseMiddlewareModes modes, Delayed<IResponseStrategy> delayedStrategy)
    {
        var delayedResponse = responseMiddlewareFactory.Create(modes, () => delayedStrategy.Value);

        var middleware = new Delayed<Middleware>(
            delayedResponse,
            delayedStrategy.GetDelay);

        return () => middleware;
    }

    public Factory<Delayed<Middleware>> CreateAction(Delayed<IAction> delayedAction)
    {
        var delayedMiddleware = new Delayed<Middleware>(
            new Middleware.Action(delayedAction.Value),
            delayedAction.GetDelay);

        return () => delayedMiddleware;
    }
}