namespace Lira.Domain.Matching.Request.Matchers;

public record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext ctx)
    {
        bool isMatch = ExpectedMethod.Method.Equals(ctx.RequestData.Method);
        return Task.FromResult(isMatch ? RequestMatchResult.Matched(name: "method", WeightValue.StaticFull) : RequestMatchResult.NotMatched);
    }
}
