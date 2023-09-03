namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating.Impl.Create;

internal class NowUtc : IObjectTextPart
{
    public static string Name => "now.utc";
    
    public object Get(RequestData request) => DateTime.UtcNow;
}
