namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Date : IObjectTextPart
{
    public static string Name => "date";
    
    public object Get(RequestData request)
    {
        var now = DateTime.Now;
        var from = now.AddYears(-10);
        var ticks = Random.Shared.NextInt64(from.Ticks, now.Ticks);

        return new DateTime(ticks);
    }
}
