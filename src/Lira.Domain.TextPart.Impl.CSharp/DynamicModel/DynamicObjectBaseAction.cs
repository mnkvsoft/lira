// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public abstract class DynamicObjectBaseAction : DynamicObjectWithDeclaredPartsBase
{
    protected DynamicObjectBaseAction(Dependencies dependencies) : base(dependencies)
    {
    }

    protected ICache cache => Cache;
}