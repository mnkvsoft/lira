using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Types;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public abstract class DynamicObjectBase
{
    public record DependenciesBase(
        Cache Cache,
        IRangesProvider RangesProvider,
        ICustomDictsProvider CustomDictsProvider,
        IDeclaredItemsProvider DeclaredItemsProvider,
        ILoggerFactory LoggerFactory);

    protected readonly IDeclaredItemsProvider DeclaredItemsProvider;
    protected readonly Cache Cache;
    private readonly ILogger _logger;

    // ReSharper disable once UnusedMember.Global
    protected const string NewLine = Constants.NewLine;

    private readonly IRangesProvider _rangesProvider;
    private readonly ICustomDictsProvider _customDictsProvider;

    protected DynamicObjectBase(DependenciesBase dependencies)
    {
        Cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
        _customDictsProvider = dependencies.CustomDictsProvider;
        DeclaredItemsProvider = dependencies.DeclaredItemsProvider;
        _logger = dependencies.LoggerFactory.CreateLogger("user");
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }

    protected CustomDic GetDic(string dicName)
    {
        return _customDictsProvider.GetCustomDic(dicName);
    }

    protected VariablesWriter GetVariablesWriter(RuleExecutingContext context, bool readOnly)
    {
        return new VariablesWriter(context, DeclaredItemsProvider, readOnly);
    }

    public void log(string message) => _logger.LogInformation(message);
}