using Lira.Domain.Generating.Writers;

namespace Lira.Domain.Caching;

public record NormalWithCachingResponseStrategy(
    TimeSpan? Delay,
    int Code,
    BodyGenerator? BodyGenerator,
    HeadersGenerator? HeadersGenerator,
    IResponseCache ResponseCache,
    IKeyExtractor KeyExtractor) : ResponseStrategy(Delay)
{
    protected override async Task ExecuteInternal(HttpContextData httpContextData)
    {
        var context = httpContextData.RuleExecutingContext;

        var headers = HeadersGenerator?.Create(context).ToArray();
        var bodyParts = BodyGenerator?.Create(context).ToArray();

        var responseData = new ResponseData(Code, bodyParts, headers);
        ResponseCache.Add(KeyExtractor.ExtractKey(), responseData);

        await httpContextData.Response.Write(responseData);
    }
}