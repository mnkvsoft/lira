namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Now : IGlobalGeneratingFunction
{
    public static string Name => "now";

    public object? Generate(RequestData request) => Generate();

    public object? Generate() => DateTime.Now;
}
