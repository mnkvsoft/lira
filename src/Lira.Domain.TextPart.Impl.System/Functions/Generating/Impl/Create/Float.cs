using Lira.Common;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Float : WithRangeArgumentFunction<decimal>, IObjectTextPart
{
    public override string Name => "float";
    public override bool ArgumentIsRequired => false;
    private Interval<decimal> _interval = new(0.01m, 10_000);

    public dynamic? Get(RuleExecutingContext context) => Math.Round(Random.Shared.NextDecimal(_interval), 2);


    public override void SetArgument(Interval<decimal> argument)
    {
        _interval = argument;
    }
}
