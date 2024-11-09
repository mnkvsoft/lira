namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class Date : FunctionBase, IObjectTextPart
{
    public override string Name => "date";

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.Now;
        var from = now.AddYears(-1);
        var ticks = Random.Shared.NextInt64(from.Ticks, now.Ticks);

        return Task.FromResult<dynamic?>(new DateTime(ticks));
    }
}