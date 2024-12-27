using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public abstract class DynamicObjectBase
{
    public record DependenciesBase(
        Cache Cache,
        IRangesProvider RangesProvider,
        IDeclaredPartsProvider DeclaredPartsProvider);

    protected readonly IDeclaredPartsProvider DeclaredPartsProvider;
    protected readonly Cache Cache;

    // ReSharper disable once UnusedMember.Global
    protected const string NewLine = Constants.NewLine;

    private readonly IRangesProvider _rangesProvider;

    protected DynamicObjectBase(DependenciesBase dependencies)
    {
        Cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
        DeclaredPartsProvider = dependencies.DeclaredPartsProvider;
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }

    protected VariablesWriter GetVariablesWriter(RuleExecutingContext context, bool readOnly)
    {
        return new VariablesWriter(context, DeclaredPartsProvider, readOnly);
    }
}