namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class NowUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "now.utc";
    
    public object Get(RequestData request) => DateTime.UtcNow;
}
