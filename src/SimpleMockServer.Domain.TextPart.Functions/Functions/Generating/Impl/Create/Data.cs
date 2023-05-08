using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;
internal class Data : DataBase, IGlobalObjectTextPart
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public object Get(RequestData request) => Get();

    public object Get()
    {
        return GetRange().NextValue().ToString()!;
    }
}
