using Microsoft.AspNetCore.Http;
using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer.Domain.Matching.Matchers.Method;

internal record MethodRequestMatcher(string ExpectedMethod) : IRequestMatcher
{
    public bool IsMatch(HttpRequest request)
    {
        bool result = ExpectedMethod.Equals(request.Method, StringComparison.OrdinalIgnoreCase);
        return result;
    }
}