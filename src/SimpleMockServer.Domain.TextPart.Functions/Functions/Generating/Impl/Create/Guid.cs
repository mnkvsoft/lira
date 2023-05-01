namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Guid : IGlobalGeneratingFunction
{
    public static string Name => "guid";

    public object? Generate(RequestData request) => Generate();

    public object? Generate() => System.Guid.NewGuid();
}
