namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Guid : FunctionBase, IObjectTextPart
{
    public override string Name => "guid";

    public object Get(RequestData _) => global::System.Guid.NewGuid();
}
