using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Seq : IGeneratingPrettyFunction
{
    private static long _counter;

    public static string Name => "seq";

    public object? Generate(RequestData request)
    {
        return Interlocked.Increment(ref _counter);
    }
}
