using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Matching.String.Impl;

internal class Data : DataBase, IStringMatchPrettyFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public bool IsMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var range = GetRange();

        var isMatch = range.IsBelong(value);

        return isMatch;
    }
}
