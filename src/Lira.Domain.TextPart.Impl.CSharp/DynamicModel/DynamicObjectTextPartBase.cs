namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
public class DynamicObjectTextPartBase
{
    private readonly IDeclaredPartsProvider _declaredPartsProvider;

    public DynamicObjectTextPartBase(IDeclaredPartsProvider declaredPartsProvider)
    {
        _declaredPartsProvider = declaredPartsProvider;
    }

    public dynamic? GetDeclaredPart(string name, RequestData request) => _declaredPartsProvider.Get(name).Get(request);
}