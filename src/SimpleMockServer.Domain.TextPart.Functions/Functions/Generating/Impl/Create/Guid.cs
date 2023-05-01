using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Guid : IGeneratingPrettyFunction
{
    public static string Name => "guid";

    public object? Generate(RequestData request)
    {
        return System.Guid.NewGuid();
    }
}
