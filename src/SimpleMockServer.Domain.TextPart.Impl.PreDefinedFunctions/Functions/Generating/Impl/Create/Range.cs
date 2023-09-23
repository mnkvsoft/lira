using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;
internal class Range : DataBase, IObjectTextPart
{
    public Range(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "range";

    public object Get(RequestData request) => GetRange().NextValue().ToString()!;
}
