namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Guid : IObjectTextPart
{
    public static string Name => "guid";

    public object Get(RequestData _) => global::System.Guid.NewGuid();
}
