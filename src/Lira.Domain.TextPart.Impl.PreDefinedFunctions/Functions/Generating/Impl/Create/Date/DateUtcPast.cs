namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class DateUtcPast : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.past";
    
    public object Get(RequestData request)
    {
        var now = DateTime.UtcNow;
        var to = now.AddYears(-1);
        var from = to.AddYears(-10);
        
        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}