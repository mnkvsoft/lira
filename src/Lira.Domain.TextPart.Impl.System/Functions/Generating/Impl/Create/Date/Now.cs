namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class Now : FunctionBase, IObjectTextPart
{
    public override string Name => "now";
    public ReturnType ReturnType => ReturnType.Date;
    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return DateTime.Now;
    }
}
