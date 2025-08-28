namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Guid : FunctionBase, IObjectTextPart
{
    public override string Name => "guid";

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return global::System.Guid.NewGuid();
    }

    public ReturnType ReturnType => ReturnType.Guid;
}
