namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtcFuture : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.future";

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var from = now.AddYears(1);
        var to = from.AddYears(10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return Task.FromResult<dynamic?>(new DateTime(ticks, DateTimeKind.Utc));
    }
}