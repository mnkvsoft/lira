// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public abstract class DynamicObjectBaseGenerate : DynamicObjectWithDeclaredPartsBase
{
    protected DynamicObjectBaseGenerate(DependenciesBase dependencies) : base(dependencies)
    {
    }

    protected IReadonlyCache cache => Cache;

    protected dynamic range(string rangeName)
    {
        return GetRange(rangeName).NextValue();
    }

    protected async IAsyncEnumerable<dynamic?> Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await foreach (var obj in part.Get(context))
            {
                if(i > 0)
                    yield return separator;
                yield return obj;
            }
        }
    }
}