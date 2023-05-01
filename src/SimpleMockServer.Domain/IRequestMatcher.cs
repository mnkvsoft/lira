namespace SimpleMockServer.Domain;

public interface IRequestMatcher
{
    Task<bool> IsMatch(RequestData request);
}