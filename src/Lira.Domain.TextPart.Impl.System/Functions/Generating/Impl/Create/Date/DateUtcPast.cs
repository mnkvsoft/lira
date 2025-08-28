namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtcPast : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.past";
    public ReturnType ReturnType => ReturnType.Date;

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var to = now.AddYears(-1);
        var from = to.AddYears(-10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        yield return new DateTime(ticks, DateTimeKind.Utc);
    }
}