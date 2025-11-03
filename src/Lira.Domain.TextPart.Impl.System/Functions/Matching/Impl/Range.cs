using Lira.Domain.DataModel;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Range : RangeBase, IMatchFunctionTyped
{
    public Range(IRangesProvider dataProvider) : base(dataProvider)
    {
    }

    public override string Name => "range";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    public ReturnType ValueType => ReturnType.String;


    public bool IsMatch(RuleExecutingContext context, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        return range.ValueIsBelong(value);
    }
}
