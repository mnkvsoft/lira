namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Now : IObjectTextPart
{
    public static string Name => "now";
    public object Get(RequestData request) => DateTime.Now;
}
