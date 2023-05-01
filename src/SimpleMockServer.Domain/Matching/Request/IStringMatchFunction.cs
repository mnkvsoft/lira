namespace SimpleMockServer.Domain.Matching.Request;

public interface IStringMatchFunction
{
    bool IsMatch(string? value);
}
