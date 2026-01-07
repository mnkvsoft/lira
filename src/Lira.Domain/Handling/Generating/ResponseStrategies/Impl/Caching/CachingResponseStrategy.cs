using Lira.Common;
using Lira.Common.Exceptions;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

class CachingResponseStrategy(
    TimeSpan cachingTime,
    IRuleKeyExtractor ruleKeyExtractor,
    ResponseCache responseCache,
    Factory<IResponseStrategy> originalResponseStrategyFactory)
    : IResponseStrategy, IRequestMatcher
{
    private readonly Guid _id = Guid.NewGuid();
    private static readonly Type ContextItemsKey = typeof(CachingResponseStrategy);

    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext ctx)
    {
        var key = GetKey(ctx);

        if (!responseCache.TryGet(key, out var requestHandleResult))
            return Task.FromResult(RequestMatchResult.NotMatched);

        ctx.Items.Add(ContextItemsKey, requestHandleResult);

        return Task.FromResult(RequestMatchResult.Matched("caching", WeightValue.CustomCode));
    }

    async Task IResponseStrategy.Handle(RuleExecutingContext ctx, IResponseWriter responseWriter)
    {
        if (ctx.TryGetItem<Response>(ContextItemsKey, out var requestHandleResult))
        {
            var responseStrategy = CreateResponseStrategy(requestHandleResult);
            await responseStrategy.Handle(ctx, responseWriter);
        }
        else
        {
            var savingResponseWriter = new SavingResponseWriter(responseWriter);
            var strategy = originalResponseStrategyFactory();
            await strategy.Handle(ctx, savingResponseWriter);

            var handleResult = savingResponseWriter.GetRequestHandleResult();
            responseCache.Add(GetKey(ctx), handleResult, cachingTime);
        }
    }

    private string GetKey(RuleExecutingContext ruleExecutingContext)
    {
        string ruleKey = ruleKeyExtractor.Extract(ruleExecutingContext);
        return $"{_id}-{ruleKey}";
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