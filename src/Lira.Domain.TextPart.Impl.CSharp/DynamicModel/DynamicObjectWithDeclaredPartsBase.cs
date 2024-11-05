// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectWithDeclaredPartsBase : DynamicObjectBase
{
    private readonly IDeclaredPartsProvider _declaredPartsProvider;

    public record Dependencies(
        DependenciesBase Base,
        IDeclaredPartsProvider DeclaredPartsProvider);

    protected DynamicObjectWithDeclaredPartsBase(Dependencies dependencies) : base(dependencies.Base)
    {
        _declaredPartsProvider = dependencies.DeclaredPartsProvider;
    }

    protected dynamic? GetDeclaredPart(string name, RuleExecutingContext context)
    {
        dynamic? part = _declaredPartsProvider.Get(name).Get(context);
        return part;
    }

    protected IObjectTextPart GetDeclaredPart(string name)
    {
        return _declaredPartsProvider.Get(name);
    }

    protected string Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        return string.Join(separator,
            Enumerable.Repeat("", count)
                .Select(_ => part.Get(context)?.ToString() ?? ""));
    }
}