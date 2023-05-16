using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String.Impl;

internal class Data : DataBase, IStringMatchPrettyFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;

    public bool IsMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        var isMatch = range.IsBelong(value);

        return isMatch;
    }
}
