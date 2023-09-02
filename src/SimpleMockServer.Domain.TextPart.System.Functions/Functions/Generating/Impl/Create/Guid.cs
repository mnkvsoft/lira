namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating.Impl.Create;

internal class Guid : IGlobalObjectTextPart
{
    public static string Name => "guid";

    public object Get(RequestData request) => Get();

    public object Get() => global::System.Guid.NewGuid();
}
