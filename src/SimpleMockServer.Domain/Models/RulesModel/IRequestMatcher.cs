namespace SimpleMockServer.Domain.Models.RulesModel;

public interface IRequestMatcher
{
    Task<bool> IsMatch(RequestData request);
}