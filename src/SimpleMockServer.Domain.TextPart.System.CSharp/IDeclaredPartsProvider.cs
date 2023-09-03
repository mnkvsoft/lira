namespace SimpleMockServer.Domain.TextPart.System.CSharp;

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    bool IsAllowInName(char c);
    bool IsStartPart(char c);
}