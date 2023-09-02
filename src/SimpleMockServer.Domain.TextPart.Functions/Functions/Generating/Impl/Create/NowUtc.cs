namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class NowUtc : IGlobalObjectTextPart
{
    public static string Name => "now.utc";
    
    public object Get(RequestData request) => Get();

    public object Get() => DateTime.UtcNow;
}
