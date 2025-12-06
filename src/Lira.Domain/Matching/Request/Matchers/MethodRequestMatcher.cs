namespace Lira.Domain.Matching.Request.Matchers;

public record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    Task<RequestMatchResult> IRequestMatcher.IsMatch(RuleExecutingContext context)
    {
        bool isMatch = ExpectedMethod.Method.Equals(context.RequestData.Method);
        return Task.FromResult(isMatch ? RequestMatchResult.Matched(name: "method", WeightValue.StaticFull) : RequestMatchResult.NotMatched);
    }
}
