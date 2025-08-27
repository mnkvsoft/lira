namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc";
    public ReturnType ReturnType => ReturnType.Date;
    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var from = now.AddYears(-10);
        var ticks = Random.Shared.NextInt64(from.Ticks, now.Ticks);

        yield return new DateTime(ticks, DateTimeKind.Utc);
    }
}
