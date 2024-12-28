namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class ДDateFuture : FunctionBase, IObjectTextPart
{
    public override string Name => "date.future";

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.Now;
        var from = now.AddYears(1);
        var to = from.AddYears(10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return Task.FromResult<dynamic?>(new DateTime(ticks));
    }
}