using System.Collections.Immutable;

namespace Lira.Domain.Matching.Request.Matchers;

public record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    internal Task<RequestMatchResult> IsMatch(RequestData request)
    {
        bool isMatch = ExpectedMethod.Method.Equals(request.Method);
        return Task.FromResult(isMatch ? RequestMatchResult.Matched(WeightValue.StaticFull, ImmutableDictionary<string, string?>.Empty) : RequestMatchResult.NotMatched);
    }
}
