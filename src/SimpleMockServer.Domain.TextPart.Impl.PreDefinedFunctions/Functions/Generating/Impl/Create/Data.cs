using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Generating.Impl.Create;
internal class Data : DataBase, IObjectTextPart
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "range";

    public object Get(RequestData request) => GetRange().NextValue().ToString()!;
}
