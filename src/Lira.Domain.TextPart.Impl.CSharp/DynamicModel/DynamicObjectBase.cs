namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
public class DynamicObjectBase
{
    private readonly IDeclaredPartsProvider _declaredPartsProvider;

    public DynamicObjectBase(IDeclaredPartsProvider declaredPartsProvider)
    {
        _declaredPartsProvider = declaredPartsProvider;
    }

    public dynamic? GetDeclaredPart(string name, RequestData request) => _declaredPartsProvider.Get(name).Get(request);
}