using Lira.Domain.Matching.Request;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.Impl;

internal class Range : DataBase, IMatchFunctionPreDefined
{
    public Range(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public override string Name => "range";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;


    public bool IsMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        var isMatch = range.ValueIsBelong(value);

        return isMatch;
    }
}
