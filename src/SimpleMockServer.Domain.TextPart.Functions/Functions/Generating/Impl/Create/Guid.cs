namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Guid : IGlobalObjectTextPart
{
    public static string Name => "guid";

    public object Get(RequestData request) => Get();

    public object Get() => System.Guid.NewGuid();
}
