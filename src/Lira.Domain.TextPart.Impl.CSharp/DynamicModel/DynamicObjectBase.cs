using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public abstract class DynamicObjectBase
{
    public record DependenciesBase(
        Cache Cache,
        IRangesProvider RangesProvider);

    protected readonly Cache Cache;

    // ReSharper disable once UnusedMember.Global
    protected const string NewLine = Constants.NewLine;

    private readonly IRangesProvider _rangesProvider;

    protected DynamicObjectBase(DependenciesBase dependencies)
    {
        Cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }
}