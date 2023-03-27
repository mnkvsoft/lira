using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Method;

internal record MethodRequestMatcher(HttpMethod ExpectedMethod) : IRequestMatcher
{
    public Task<bool> IsMatch(HttpRequest request)
    {
        bool result = ExpectedMethod.Method.Equals(request.Method);
        return Task.FromResult(result);
    }
}