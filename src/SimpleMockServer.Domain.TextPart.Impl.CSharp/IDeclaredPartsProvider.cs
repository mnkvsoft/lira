namespace SimpleMockServer.Domain.TextPart.Impl.CSharp;

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    bool IsAllowInName(char c);
    bool IsStartPart(char c);
}