// ReSharper disable UnusedMember.Global

using System.Text;

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

    protected Task<dynamic?> GetDeclaredPart(string name, RuleExecutingContext context)
    {
        return _declaredPartsProvider.Get(name).Get(context);
    }

    protected IObjectTextPart GetDeclaredPart(string name)
    {
        return _declaredPartsProvider.Get(name);
    }

    protected async Task<string> Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        var parts = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var obj = await part.Get(context);
            parts.Add(obj?.ToString() ?? string.Empty);
        }

        return string.Join(separator, parts);
    }
}