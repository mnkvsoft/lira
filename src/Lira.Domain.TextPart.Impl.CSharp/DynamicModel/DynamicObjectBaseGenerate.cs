namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBaseGenerate : DynamicObjectBase
{
    public DynamicObjectBaseGenerate(Dependencies dependencies) : base(dependencies)
    {
    }

    protected IReadonlyCache cache => _cache;

    protected dynamic range(string rangeName)
    {
        return GetRange(rangeName).NextValue();
    }
}