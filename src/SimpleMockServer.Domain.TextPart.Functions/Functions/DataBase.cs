using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions;

abstract class DataBase : IWithStringArgumentFunction
{
    private readonly IDataProvider _dataProvider;

    protected DataBase(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    private DataName _name;
    private DataName _rangeName;

    void IWithStringArgumentFunction.SetArgument(string argument)
    {
        var (name, nameRange) = argument.SplitToTwoPartsRequired(".");

        _name = new DataName(name);
        _rangeName = new DataName(nameRange);
    }

    protected DataRange GetRange() => _dataProvider.GetData(_name).Get(_rangeName);
}