using Lira.Common.PrettyParsers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class Cache
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultLifeTimeInCache;
    private readonly static object FlagValue = new();
    
    public Cache(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _defaultLifeTimeInCache = configuration.GetValue<TimeSpan>("DefaultLifeTimeInCache");
    }

    public void setFlag(string key, string? time = null)
    {
        set(key, FlagValue, time);
    }

    public void set(string key, object value, string? time = null)
    {
        _memoryCache.Set(key, value, time != null ? PrettyTimespanParser.Parse(time) : _defaultLifeTimeInCache);
    }
    
    public bool contains(string key) => _memoryCache.TryGetValue(key, out _);
    
    public dynamic get(string key)
    {
        if (_memoryCache.TryGetValue(key, out var obj))
            return obj!;

        throw new Exception($"Object with key '{key}' not found in cache");
    }
    
    public void remove(string key) => _memoryCache.Remove(key);
}