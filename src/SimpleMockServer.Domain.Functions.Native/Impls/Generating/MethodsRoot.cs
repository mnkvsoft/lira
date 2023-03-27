using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.Domain.Functions.Native.Impls.Generating;

class GeneratingFunctionRoot
{
    private readonly IDataProvider _dataProvider;

    public GeneratingFunctionRoot(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public object Data(string name)
    {
        return _dataProvider.GetData(new DataName(name)).GetDefault().NextValue();
    }

    public object Data(string name, string range)
    {
        return _dataProvider.GetData(new DataName(name)).Get(new DataName(range)).NextValue();
    }

    public DateTime Now()
    {
        return DateTime.Now;
    }

    public Guid Guid()
    {
        return System.Guid.NewGuid();
    }
}