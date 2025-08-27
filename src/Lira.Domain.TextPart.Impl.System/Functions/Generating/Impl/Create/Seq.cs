namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Seq : FunctionBase, IObjectTextPart
{
    private readonly SystemSequence _sequence;

    public Seq(SystemSequence sequence) => _sequence = sequence;

    public override string Name => "seq";

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return _sequence.Next();
    }

    public ReturnType ReturnType => ReturnType.Int;
}
