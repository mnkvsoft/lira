namespace SimpleMockServer.Domain.Matching.Matchers.Body;

interface IBodyPattern
{
    bool IsMatch(string body);
}