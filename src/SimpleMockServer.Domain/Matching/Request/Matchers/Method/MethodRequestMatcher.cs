namespace SimpleMockServer.Domain.Matching.Request.Matchers.Method;

internal record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    public Task<bool> IsMatch(RequestData request)
    {
        var result = ExpectedMethod.Method.Equals(request.Method);
        return Task.FromResult(result);
    }
}