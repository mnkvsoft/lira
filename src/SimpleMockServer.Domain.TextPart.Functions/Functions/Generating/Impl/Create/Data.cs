using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;
internal class Data : DataBase, IGeneratingPrettyFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public object? Generate(RequestData request)
    {
        return GetRange().NextValue().ToString();
    }
}
