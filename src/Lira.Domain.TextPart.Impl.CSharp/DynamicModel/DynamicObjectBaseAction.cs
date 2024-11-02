namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBaseAction : DynamicObjectBase
{
    public DynamicObjectBaseAction(Dependencies dependencies) : base(dependencies)
    {
    }

    protected ICache cache => Cache;
}
