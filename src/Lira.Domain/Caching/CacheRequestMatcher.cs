// using System.Collections.Immutable;
// using Lira.Domain.Matching.Request;
//
// namespace Lira.Domain.Caching;
//
// internal class CacheRequestMatcher : IRequestMatcher
// {
//     private readonly IResponseCache _cache;
//     private readonly IKeyExtractor _keyExtractor;
//
//     public CacheRequestMatcher(IResponseCache cache, IKeyExtractor keyExtractor)
//     {
//         _cache = cache;
//         _keyExtractor = keyExtractor;
//     }
//
//     public Task<RequestMatchResult> IsMatch(RequestContext context)
//     {
//         var key = _keyExtractor.ExtractKey();
//         if(_cache.Contains(key))
//         {
//             return Task.FromResult(RequestMatchResult.Matched(
//                 name: "cache",
//                 weight: WeightValue.Cache,
//                 matchedValues: ImmutableDictionary<string, string?>.Empty));
//         }
//         return Task.FromResult(RequestMatchResult.NotMatched);
//     }
// }
