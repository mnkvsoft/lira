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
}