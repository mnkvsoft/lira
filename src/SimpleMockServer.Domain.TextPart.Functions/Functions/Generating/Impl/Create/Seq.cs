namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Seq : IGlobalGeneratingFunction
{
    private static long _counter;

    public static string Name => "seq";

    public object? Generate(RequestData request) => Generate();

    public object? Generate() => Interlocked.Increment(ref _counter);
}
