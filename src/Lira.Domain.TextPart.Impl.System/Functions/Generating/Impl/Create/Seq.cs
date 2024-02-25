namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Seq : FunctionBase, IObjectTextPart
{
    private static long _counter;

    public override string Name => "seq";

    public dynamic? Get(RuleExecutingContext context) => Interlocked.Increment(ref _counter);
}
