namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class NowUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "now.utc";
    public ReturnType ReturnType => ReturnType.Date;
    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return DateTime.UtcNow;
    }
}
