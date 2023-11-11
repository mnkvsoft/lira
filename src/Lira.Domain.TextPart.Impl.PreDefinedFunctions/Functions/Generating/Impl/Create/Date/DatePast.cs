namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class DatePast : FunctionBase, IObjectTextPart
{
    public override string Name => "date.past";
    
    public object Get(RequestData request)
    {
        var now = DateTime.Now;
        var to = now.AddYears(-1);
        var from = to.AddYears(-10);
        
        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return new DateTime(ticks);
    }
}