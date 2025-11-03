namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create.Date;

internal class DateUtcPast : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc.past";
    public Type Type => ExplicitType.Date.DotnetType;

    public dynamic Get(RuleExecutingContext context)
    {
        var now = DateTime.UtcNow;
        var to = now.AddYears(-1);
        var from = to.AddYears(-10);

        var ticks = Random.Shared.NextInt64(from.Ticks, to.Ticks);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}