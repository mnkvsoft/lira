namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class NowUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "now.utc";
    public Type Type => ExplicitType.Date.DotnetType;
    public dynamic Get(RuleExecutingContext context) => DateTime.UtcNow;
}
