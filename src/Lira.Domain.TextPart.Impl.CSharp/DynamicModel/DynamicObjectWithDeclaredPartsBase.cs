// ReSharper disable UnusedMember.Global

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectWithDeclaredPartsBase : DynamicObjectBase
{
    protected DynamicObjectWithDeclaredPartsBase(DependenciesBase dependencies) : base(dependencies){}

    protected Task<dynamic?> GetDeclaredPart(string name, RuleExecutingContext context)
    {
        return DeclaredPartsProvider.Get(name).Get(context);
    }

    protected IObjectTextPart GetDeclaredPart(string name)
    {
        return DeclaredPartsProvider.Get(name);
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