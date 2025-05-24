namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateFuture : FunctionBase, IObjectTextPart
{
    public override string Name => "date.future";
    public ReturnType ReturnType => ReturnType.Date;

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var now = DateTime.Now;
        var from = now.AddYears(1);
        var to = from.AddYears(10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        yield return new DateTime(ticks);
    }
}