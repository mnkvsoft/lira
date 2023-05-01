using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;
internal class Data : DataBase, IGlobalGeneratingFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public object? Generate(RequestData request) => Generate();

    public object? Generate()
    {
        return GetRange().NextValue().ToString();
    }
}
