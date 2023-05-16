namespace SimpleMockServer.Domain.Matching.Request.Matchers.Method;

public record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    public Task<RequestMatchResult> IsMatch(RequestData request)
    {
        bool isMatch = ExpectedMethod.Method.Equals(request.Method);
        return Task.FromResult(isMatch ? RequestMatchResult.Matched(WeightValue.StaticFull) : RequestMatchResult.NotMatched);
    }
}
