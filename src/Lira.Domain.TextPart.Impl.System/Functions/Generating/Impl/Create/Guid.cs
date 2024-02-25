namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Guid : FunctionBase, IObjectTextPart
{
    public override string Name => "guid";

    public dynamic? Get(RuleExecutingContext context) => global::System.Guid.NewGuid();
}
