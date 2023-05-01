using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Now : IGeneratingPrettyFunction
{
    public static string Name => "now";

    public object? Generate(RequestData request)
    {
        return DateTime.Now;
    }
}
