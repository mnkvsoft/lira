namespace SimpleMockServer.Domain.Matching.Request.Matchers.Body;

public interface IBodyExtractFunction
{
    string? Extract(string? body);
}
