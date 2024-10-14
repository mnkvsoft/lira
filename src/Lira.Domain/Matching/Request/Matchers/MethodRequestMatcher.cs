using System.Collections.Immutable;

namespace Lira.Domain.Matching.Request.Matchers;

public record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    Task<RequestMatchResult> IRequestMatcher.IsMatch(RequestContext context)
    {
        bool isMatch = ExpectedMethod.Method.Equals(context.RequestData.Method);
        return Task.FromResult(isMatch ? RequestMatchResult.Matched(name: "method", WeightValue.StaticFull, ImmutableDictionary<string, string?>.Empty) : RequestMatchResult.NotMatched);
    }
}
