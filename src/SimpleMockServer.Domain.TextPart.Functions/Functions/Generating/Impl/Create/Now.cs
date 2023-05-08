namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Now : IGlobalObjectTextPart
{
    public static string Name => "now";

    public object Get(RequestData request) => Get();

    public object Get() => DateTime.Now;
}
