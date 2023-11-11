namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class Now : FunctionBase, IObjectTextPart
{
    public override string Name => "now";
    public object Get(RequestData request) => DateTime.Now;
}
