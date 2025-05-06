namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtcPast : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.past";
    public ReturnType ReturnType => ReturnType.Date;

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var to = now.AddYears(-1);
        var from = to.AddYears(-10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return Task.FromResult<dynamic?>(new DateTime(ticks, DateTimeKind.Utc));
    }
}