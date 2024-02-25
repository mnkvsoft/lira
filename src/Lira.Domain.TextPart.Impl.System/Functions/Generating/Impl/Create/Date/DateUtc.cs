namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc";
    
    public dynamic? Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var from = now.AddYears(-10);
        var ticks = Random.Shared.NextInt64(from.Ticks, now.Ticks);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}
