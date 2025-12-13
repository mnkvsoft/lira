using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Lira.Domain.Handling.Generating;

abstract record RequestHandleResult
{
    public record Response(int StatusCode, IReadOnlyCollection<Header>? Headers, string? Body) : RequestHandleResult;

    public record Fault : RequestHandleResult
    {
        public static readonly Fault Instance = new();
    }
}

record RuleHistoryItem(DateTime HandleTime, RequestData Request, RequestHandleResult Result);

internal interface IHandledRuleHistoryStorage
{
    IEnumerable<RuleHistoryItem> GetHistory(RuleName ruleName);
}

class HandledRuleHistoryStorage : IHandledRuleHistoryStorage
{
    private readonly MemoryCache _memoryCache;
    private readonly TimeSpan _lifeTime;
    private long _counter = 0;

    private const string Suffix = "__suffix__";
    private const string Prefix = "__prefix__";

    public HandledRuleHistoryStorage(MemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _lifeTime = configuration.GetValue<TimeSpan>("RuleHistoryLifeTime");
    }


    public void Add(RuleName ruleName, DateTime executeTime, RequestData requestData, RequestHandleResult handleResult)
    {
        _memoryCache.Set(
            GetCacheKeyPattern(ruleName) + Interlocked.Increment(ref _counter),
            new RuleHistoryItem(executeTime, requestData, handleResult),
            _lifeTime);
    }

    public IEnumerable<RuleHistoryItem> GetHistory(RuleName ruleName)
    {
        foreach (string key in _memoryCache.Keys)
        {
            var pattern = GetCacheKeyPattern(ruleName);
            if (key.StartsWith(pattern))
            {
                if (_memoryCache.TryGetValue(key, out var item))
                    yield return (RuleHistoryItem)(item ?? throw new Exception("item is null"));
            }
        }
    }

    private string GetCacheKeyPattern(RuleName ruleName) => Suffix + ruleName.Value + Prefix;
}