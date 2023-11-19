using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;
internal class Range : DataBase, IObjectTextPart
{
    public Range(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public override string Name => "range";
    public override bool ArgumentIsRequired => true;
    public object Get(RequestData request) => GetRange().NextValue().ToString()!;
}