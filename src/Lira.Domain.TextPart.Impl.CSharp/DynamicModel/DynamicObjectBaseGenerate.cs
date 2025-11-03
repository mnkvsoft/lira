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
}