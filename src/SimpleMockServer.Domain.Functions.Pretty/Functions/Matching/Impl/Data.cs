using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Matching.Impl;

internal class Data : DataBase, IMatchPrettyFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public bool IsMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        DataRange range = GetRange();

        bool isMatch = range.IsBelong(value);

        return isMatch;
    }
}