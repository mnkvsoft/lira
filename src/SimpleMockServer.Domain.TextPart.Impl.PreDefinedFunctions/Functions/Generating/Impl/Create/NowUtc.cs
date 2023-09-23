namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class NowUtc : IObjectTextPart
{
    public static string Name => "now.utc";
    
    public object Get(RequestData request) => DateTime.UtcNow;
}
