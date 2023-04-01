using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.String.Impl;

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
