using Lira.Domain.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Lira.Common;

namespace Lira.Domain.Matching.Conditions;

public interface IRequestStatisticStorage
{
    Task<RequestStatistic> Add(RuleExecutingContext context);
}

class RequestStatisticStorage(IMemoryCache cache) : IRequestStatisticStorage
{
    private static readonly object Flag = new();

    public async Task<RequestStatistic> Add(RuleExecutingContext context)
    {
        var hash = await GetHash(context.RequestData);

        var statistic = cache.Get<RequestStatistic>(hash);

        if (statistic != null)
        {
            if (context.Items.TryAdd(GetType(), Flag))
            {
                statistic.Add();
            }
        }
        else
        {
            statistic = new RequestStatistic();
            context.Items.TryAdd(GetType(), Flag);
            statistic.Add();
            cache.Set(
                hash,
                statistic,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
        }

        return statistic;
    }

    private async Task<Hash> GetHash(RequestData request)
    {
        using var memoryStream = new MemoryStream();
        await using var sw = new StreamWriter(memoryStream);

        await sw.WriteAsync(request.Method);
        await sw.WriteAsync(request.Path.ToString());
        await sw.WriteAsync(request.QueryString.ToString());
        await sw.WriteAsync(request.ReadBody());
        await sw.WriteAsync(string.Join("", request.Headers.Select(x => x.Key + x.Value)));

        await sw.FlushAsync();

        await request.Body.CopyToAsync(memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);
        return Sha1.Create(memoryStream);
    }
}

