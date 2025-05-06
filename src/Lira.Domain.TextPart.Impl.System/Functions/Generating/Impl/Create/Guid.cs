namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Guid : FunctionBase, IObjectTextPart
{
    public override string Name => "guid";

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(global::System.Guid.NewGuid());
    public ReturnType ReturnType => ReturnType.Guid;
}
