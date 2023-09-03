namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating.Impl.Create;

internal class Guid : IObjectTextPart
{
    public static string Name => "guid";

    public object Get(RequestData _) => global::System.Guid.NewGuid();
}
