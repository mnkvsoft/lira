using Lira.Domain.DataModel;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Range : RangeBase, IMatchFunctionSystem
{
    public Range(IRangesProvider dataProvider) : base(dataProvider)
    {
    }

    public override string Name => "range";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;


    public bool IsMatch(RuleExecutingContext context, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        var isMatch = range.ValueIsBelong(value);

        return isMatch;
    }
}
