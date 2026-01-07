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
            factory = () => new CachingResponseStrategy(
                enabled.Time,
                enabled.RuleKeyExtractor,
                responseCache,
                responseStrategyFactory);
        }

        return new Middleware.Response(
            new WriteStatDependencies(handledRuleHistoryStorage, modes.WriteHistory),
            factory);
    }
}