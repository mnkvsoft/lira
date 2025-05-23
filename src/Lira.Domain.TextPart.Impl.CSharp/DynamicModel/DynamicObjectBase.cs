using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public abstract class DynamicObjectBase
{
    public record DependenciesBase(
        Cache Cache,
        IRangesProvider RangesProvider,
        IDeclaredItemsProvider DeclaredItemsProvider);

    protected readonly IDeclaredItemsProvider DeclaredItemsProvider;
    protected readonly Cache Cache;

    // ReSharper disable once UnusedMember.Global
    protected const string NewLine = Constants.NewLine;

    private readonly IRangesProvider _rangesProvider;

    protected DynamicObjectBase(DependenciesBase dependencies)
    {
        Cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
        DeclaredItemsProvider = dependencies.DeclaredItemsProvider;
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }

    protected VariablesWriter GetVariablesWriter(RuleExecutingContext context, bool readOnly)
    {
        return new VariablesWriter(context, DeclaredItemsProvider, readOnly);
    }

    public static Json json(string json)
    {
        return Json.Parse(json);
    }
}