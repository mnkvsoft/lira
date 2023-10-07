namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    IReadOnlyCollection<string> GetAllNamesDeclared();
}