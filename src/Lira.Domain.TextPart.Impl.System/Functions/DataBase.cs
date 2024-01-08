using Lira.Common.Extensions;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.System.Functions;

abstract class RangeBase : WithArgumentFunction<string>
{
    private readonly IDataProvider _dataProvider;
    public override bool ArgumentIsRequired => true;
    
    protected RangeBase(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    private DataName _name;
    private DataName _rangeName;

    public override void SetArgument(string argument)
    {
        var (name, nameRange) = argument.SplitToTwoPartsRequired("/");

        _name = new DataName(name);
        _rangeName = new DataName(nameRange);
    }

    protected DataRange GetRange() => _dataProvider.GetData(_name).Get(_rangeName);
}