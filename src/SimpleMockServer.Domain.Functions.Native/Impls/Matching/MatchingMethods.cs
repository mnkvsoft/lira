using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.Domain.Functions.Native.Impls.Matching;

class MatchingMethodsRoot
{
    private readonly IDataProvider _dataProvider;

    public MatchingMethodsRoot(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public bool IsData(string value, string name)
    {
        return _dataProvider.GetData(new DataName(name)).GetDefault().IsBelong(value);
    }

    public bool IsData(string value, string name, string range)
    {
        return _dataProvider.GetData(new DataName(name)).Get(new DataName(range)).IsBelong(value);
    }

    public bool IsGuid(string value)
    {
        return Guid.TryParse(value, out var id);
    }

    public bool IsAny(string value)
    {
        return true;
    }

    public bool IsNumber(string value)
    {
        return long.TryParse(value, out _);
    }

    public bool IsFloatNumber(string value)
    {
        return decimal.TryParse(value, out _);
    }
}