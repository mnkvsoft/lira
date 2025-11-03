// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public abstract class DynamicObjectBaseGenerate(DynamicObjectBase.DependenciesBase dependencies)
    : DynamicObjectWithDeclaredPartsBase(dependencies)
{
    protected IReadonlyCache cache => Cache;

    protected dynamic range(string rangeName)
    {
        return GetRange(rangeName).NextValue();
    }

    protected dynamic dic(string dicName)
    {
        return GetDic(dicName).NextValue();
    }

    protected IEnumerable<dynamic?> Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            foreach (var obj in part.Get(context))
            {
                if(i > 0)
                    yield return separator;
                yield return obj;
            }
        }
    }
}