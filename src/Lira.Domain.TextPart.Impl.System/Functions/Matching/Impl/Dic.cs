using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Dic(ICustomDictsProvider customDictsProviderProvider) : DicBase(customDictsProviderProvider), IMatchFunctionTyped
{
    public override string Name => "dic";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    public Type ValueType => DotNetType.String;

    public bool IsMatch(RuleExecutingContext _, string? value) => IsMatchTyped(_, value, out var _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = value;

        if (value is null)
            return false;

        if (CustomDic.ValueIsBelong(value))
        {
            typedValue = value;
            return true;
        }

        return false;
    }
}
