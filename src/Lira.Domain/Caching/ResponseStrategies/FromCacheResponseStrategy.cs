// namespace Lira.Domain.Caching.ResponseStrategies;
//
// public record FromCacheResponseStrategy(
//     TimeSpan? Delay,
//     IResponseCache ResponseCache,
//     IKeyExtractor KeyExtractor) : ResponseStrategy(Delay)
// {
//     protected override async Task ExecuteInternal(HttpContextData httpContextData)
//     {
//         var responseData = ResponseCache.Get(KeyExtractor.ExtractKey());
//         await httpContextData.Response.Write(responseData);
//     }
// }