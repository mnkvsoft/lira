// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectWithDeclaredPartsBase : DynamicObjectBase
{
    protected DynamicObjectWithDeclaredPartsBase(DependenciesBase dependencies) : base(dependencies){}

    protected dynamic? GetDeclaredPart(string name, RuleExecutingContext context)
    {
        return DeclaredItemsProvider.Get(name).Generate(context);
    }

    protected IObjectTextPart GetDeclaredPart(string name)
    {
        return DeclaredItemsProvider.Get(name);
    }
}