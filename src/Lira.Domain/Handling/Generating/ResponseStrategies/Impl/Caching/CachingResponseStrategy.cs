using Lira.Common;
using Lira.Common.Exceptions;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

class CachingResponseStrategy(
    TimeSpan cachingTime,
    IRuleKeyExtractor ruleKeyExtractor,
    ResponseCache responseCache,
    Factory<IResponseStrategy> originalResponseStrategyFactory)
    : IResponseStrategy
{
    readonly Guid _id = Guid.NewGuid();

    async Task IResponseStrategy.Handle(RuleExecutingContext ctx, IResponseWriter responseWriter)
    {
        var ruleKey = ruleKeyExtractor.Extract(ctx);
        var key = $"{_id}-{ruleKey}";

        if (!responseCache.TryGet(key, out var requestHandleResult))
        {
            var savingResponseWriter = new SavingResponseWriter(responseWriter);
            var strategy = originalResponseStrategyFactory();
            await strategy.Handle(ctx, savingResponseWriter);

            var handleResult = savingResponseWriter.GetRequestHandleResult();
            responseCache.Add(key, handleResult, cachingTime);
            return;
        }

        var responseStrategy = CreateResponseStrategy(requestHandleResult);
        await responseStrategy.Handle(ctx, responseWriter);
    }

    private static IResponseStrategy CreateResponseStrategy(Response response)
    {
        if (response is Response.Normal responseResult)
        {
            return new NormalResponseStrategy(
                new StaticHttCodeGenerator(responseResult.Code),
                responseResult.Body == null
                    ? null
                    : new StaticBodyGenerator(responseResult.Body),
                responseResult.Headers == null
                    ? null
                    : new StaticHeadersGenerator(responseResult.Headers));
        }

        if (response is Response.Fault)
        {
            return FaultResponseStrategy.Instance;
        }

        throw new UnsupportedInstanceType(response);
    }

    class StaticBodyGenerator(string text) : IBodyGenerator
    {
        public IEnumerable<string> Create(RuleExecutingContext context)
        {
            yield return text;
        }
    }

    class StaticHeadersGenerator(IReadOnlyDictionary<string,string?> headers) : IHeadersGenerator
    {
        public IEnumerable<Header> Create(RuleExecutingContext context)
        {
            foreach (var (name, value) in headers)
            {
                yield return new Header(name, value);
            }
        }
    }
}