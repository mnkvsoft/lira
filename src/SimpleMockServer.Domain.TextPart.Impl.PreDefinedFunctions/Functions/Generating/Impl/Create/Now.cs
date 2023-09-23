using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Now : IObjectTextPart
{
    public static string Name => "now";
    public object Get(RequestData request) => DateTime.Now;
}
