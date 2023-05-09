using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace SimpleMockServer.Domain.Matching.Conditions;

public interface IRequestStatisticStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestId">
    /// Is required so as not to add statistics for the same request from different rules
    /// </param>
    Task<RequestStatistic> Add(RequestData request, Guid requestId);
}

class RequestStatisticStorage : IRequestStatisticStorage
{
    private readonly IMemoryCache _cache;

    public RequestStatisticStorage(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<RequestStatistic> Add(RequestData request, Guid requestId)
    {
        var hash = await GetHash(request);

        var statistic = _cache.Get<RequestStatistic>(hash);

        if (statistic != null)
        {
            statistic.AddIfNotExist(requestId);
        }
        else
        {
            statistic = new RequestStatistic();
            statistic.AddIfNotExist(requestId);
            _cache.Set(
                hash,
                statistic,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
        }

        return statistic;
    }

    private async Task<string> GetHash(RequestData request)
    {
        using var memoryStream = new MemoryStream();
        await using var sw = new StreamWriter(memoryStream);

        await sw.WriteAsync(request.Method);
        await sw.WriteAsync(request.Path.ToString());
        await sw.WriteAsync(request.QueryString.ToString());
        sw.Write(request.Headers.Select(x => x.Key + x.Value));

        await request.Body.CopyToAsync(memoryStream);

        var result = GetSha(memoryStream);
        return result;
    }

    private static string GetSha(Stream stream)
    {
        using var sha1 = SHA1.Create();

        var hash = sha1.ComputeHash(stream);
        var sb = new StringBuilder(hash.Length * 2);

        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}

