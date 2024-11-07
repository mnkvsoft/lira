namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Seq : FunctionBase, IObjectTextPart
{
    private readonly Sequence _sequence;

    public Seq(Sequence sequence) => _sequence = sequence;

    public override string Name => "seq";

    public dynamic Get(RuleExecutingContext context) => _sequence.Next();
}
