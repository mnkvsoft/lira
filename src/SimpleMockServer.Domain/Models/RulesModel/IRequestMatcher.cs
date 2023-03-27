using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel;

public interface IRequestMatcher
{
    Task<bool> IsMatch(HttpRequest request);
}