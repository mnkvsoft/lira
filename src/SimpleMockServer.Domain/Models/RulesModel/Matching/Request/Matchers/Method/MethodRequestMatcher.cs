using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Method;

internal record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    public Task<bool> IsMatch(HttpRequest request)
    {
        var result = ExpectedMethod.Method.Equals(request.Method);
        return Task.FromResult(result);
    }
}