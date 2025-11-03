namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class Now : FunctionBase, IObjectTextPart
{
    public override string Name => "now";
    public Type Type => ExplicitType.Date.DotnetType;
    public dynamic Get(RuleExecutingContext context) => DateTime.Now;
}
