using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

internal class ResponseCache(IMemoryCache cache)
{
    public bool TryGet(
        string key,
        [MaybeNullWhen(false)] out Response response)
        => cache.TryGetValue(key, out response);

    public void Add(string key, Response handleResult, TimeSpan lifeTime)
        => cache.Set(key, handleResult, lifeTime);
}