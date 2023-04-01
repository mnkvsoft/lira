using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;

public interface IRequestStatisticStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestId">
    /// Is required so as not to add statistics for the same request from different rules
    /// </param>
    Task Add(HttpRequest request, Guid requestId);
    Task<RequestStatistic?> Get(HttpRequest request);
}

public class RequestStatisticStorage : IRequestStatisticStorage
{
    private readonly IMemoryCache _cache;

    public RequestStatisticStorage(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task Add(HttpRequest request, Guid requestId)
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
    }

    public async Task<RequestStatistic?> Get(HttpRequest request)
    {
        var hash = await GetHash(request);
        var result = _cache.Get<RequestStatistic>(hash);
        return result;
    }

    private async Task<string> GetHash(HttpRequest request)
    {
        using var memoryStream = new MemoryStream();
        using var sw = new StreamWriter(memoryStream);

        sw.Write(request.Method.ToString());
        sw.Write(request.Path.ToString());
        sw.Write(request.QueryString.ToString());
        sw.Write(request.Headers.Select(x => x.Key + x.Value));

        await request.Body.CopyToAsync(memoryStream);

        var result = GetSha1(memoryStream);
        return result;
    }

    private string GetSha1(Stream stream)
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

