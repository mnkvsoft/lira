namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class DateUtcFuture : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.future";
    
    public object Get(RequestData request)
    {
        var now = DateTime.UtcNow;
        var from = now.AddYears(1);
        var to = from.AddYears(10);
        
        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}