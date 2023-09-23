namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Seq : IObjectTextPart
{
    private static long _counter;

    public static string Name => "seq";

    public object Get(RequestData request) => Interlocked.Increment(ref _counter);
}
