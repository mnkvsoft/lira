using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Caching;
using Lira.Domain.Handling;
using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.ResponseStrategies;

namespace Lira.Domain;

public record ResponseMiddlewareModes(CachingMode Caching, WriteHistoryMode WriteHistory);

public interface IResponseGeneratorFactory
{
    Factory<Delayed<IResponseGenerator>> CreateResponse(
        ResponseMiddlewareModes modes,
        IReadOnlyList<Delayed<IResponseStrategy>> delayedStrategies);
}

class ResponseGeneratorFactory(ResponseMiddlewareFactory responseMiddlewareFactory) : IResponseGeneratorFactory
{
    public Factory<Delayed<IResponseGenerator>> CreateResponse(ResponseMiddlewareModes modes, IReadOnlyList<Delayed<IResponseStrategy>> delayedStrategies)
    {
        if (delayedStrategies.Count == 0)
            throw new Exception("Missing response strategies");

        if (delayedStrategies.Count == 1)
        {
            var delayedStrategy = delayedStrategies.First();
            var delayedResponse = responseMiddlewareFactory.Create(modes, () => delayedStrategy.Value);

            var middleware = new Delayed<IResponseGenerator>(
                delayedResponse,
                delayedStrategy.GetDelay);

            return () => middleware;
        }

        return () =>
        {
            var delayed = delayedStrategies.Random();
            var middleware = new Delayed<IResponseGenerator>(
                responseMiddlewareFactory.Create(modes, () => delayed.Value),
                delayed.GetDelay);
            return middleware;
        };
    }
}