namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Seq : IGlobalObjectTextPart
{
    private static long _counter;

    public static string Name => "seq";

    public object Get(RequestData request) => Get();

    public object Get() => Interlocked.Increment(ref _counter);
}
