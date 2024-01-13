using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
public class DynamicObjectBase
{
    public record Dependencies(IDeclaredPartsProvider DeclaredPartsProvider, Cache Cache, IRangesProvider RangesProvider);
    
    private readonly IDeclaredPartsProvider _declaredPartsProvider;
    protected readonly Cache cache;
    protected const string NewLine = Constants.NewLine;
    private readonly IRangesProvider _rangesProvider;

    public DynamicObjectBase(Dependencies dependencies)
    {
        _declaredPartsProvider = dependencies.DeclaredPartsProvider;
        cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
    }

    public dynamic? GetDeclaredPart(string name, RequestData request)
    {
        dynamic? part = _declaredPartsProvider.Get(name).Get(request);
        return part;
    }
    
    public IObjectTextPart GetDeclaredPart(string name)
    {
        return _declaredPartsProvider.Get(name);
    }
    
    protected string Repeat(RequestData request, IObjectTextPart part, string separator, int count)
    {
        return string.Join(separator, 
            Enumerable.Repeat("", count)
                .Select(_ => part.Get(request)?.ToString() ?? ""));
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }

}
