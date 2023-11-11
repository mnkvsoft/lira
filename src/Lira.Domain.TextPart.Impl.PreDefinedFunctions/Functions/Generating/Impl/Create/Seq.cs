namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Seq : FunctionBase, IObjectTextPart
{
    private static long _counter;

    public override string Name => "seq";

    public object Get(RequestData request) => Interlocked.Increment(ref _counter);
}
