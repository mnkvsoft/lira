using Microsoft.Extensions.Caching.Memory;

namespace Lira.Domain.Caching;

public interface IResponseCache
{
    bool Contains(string key, RuleExecutingContext context);
    void Add(string key, ResponseData responseData);
    ResponseData? Get(string key, RuleExecutingContext context);
}

// public class ResponseCache : IResponseCache
// {
//     private readonly IMemoryCache _memoryCache;
//
//     public ResponseCache(IMemoryCache memoryCache) => _memoryCache = memoryCache;
//
//     public bool Contains(string key) => _memoryCache.TryGetValue(key, out _);
//
//     // добавить время хранения
//     public void Add(string key, ResponseData responseData, TimeSpan expiration)
//     {
//         _memoryCache.Set(key, responseData, expiration);
//     }
//
//     public ResponseData? Get(string key)
//     {
//         var responseData = _memoryCache.Get(key) as ResponseData;
//     }
// }
