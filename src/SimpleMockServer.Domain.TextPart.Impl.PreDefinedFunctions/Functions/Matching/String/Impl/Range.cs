using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Range : DataBase, IStringMatchPrettyFunction
{
    public Range(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "range";

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
