using Lira.Common;
using Lira.Domain.Caching;
using Lira.Domain.Handling.Generating.History;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

namespace Lira.Domain.Handling.Generating.ResponseStrategies;

class ResponseMiddlewareFactory(
    HandledRuleHistoryStorage handledRuleHistoryStorage,
    ResponseCache responseCache)
{
    public Middleware Create(
        ResponseMiddlewareModes modes,
        Factory<IResponseStrategy> responseStrategyFactory)
    {
        var factory = responseStrategyFactory;

        if (modes.Caching is CachingMode.Enabled enabled)
        {
            var cachingStrategy = new CachingResponseStrategy(
                enabled.Time,
                enabled.RuleKeyExtractor,
                responseCache,
                responseStrategyFactory);

            factory = () => cachingStrategy;
        }

        return new Middleware.Response(
            new WriteStatDependencies(handledRuleHistoryStorage, modes.WriteHistory),
            factory);
    }
}