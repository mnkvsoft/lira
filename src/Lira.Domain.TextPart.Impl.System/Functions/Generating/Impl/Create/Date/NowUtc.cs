namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class NowUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "now.utc";

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(DateTime.UtcNow);
}
