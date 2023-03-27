namespace SimpleMockServer.Domain.Matching;

public interface IValuePattern
{
    bool IsMatch(string value);
}
