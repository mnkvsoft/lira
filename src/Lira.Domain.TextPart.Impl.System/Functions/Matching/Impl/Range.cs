using Lira.Domain.DataModel;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Range(IRangesProvider dataProvider) : RangeBase(dataProvider), IMatchFunctionTyped
{
    public override string Name => "range";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    public Type ValueType => DotNetType.String;


    public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        if (range.ValueIsBelong(value))
        {
            typedValue = value;
            return true;
        }

        return false;
    }
}
