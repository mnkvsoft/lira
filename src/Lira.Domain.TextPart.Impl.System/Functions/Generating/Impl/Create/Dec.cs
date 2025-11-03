using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Dec : WithRangeArgumentFunction<decimal>, IObjectTextPart
{
    public override string Name => "dec";
    public override bool ArgumentIsRequired => false;

    private Interval<decimal> _interval = new(0.01m, 10_000);

    public dynamic Get(RuleExecutingContext context) => Math.Round(Random.Shared.NextDecimal(_interval), 2);

    public Type Type => ExplicitType.Decimal.DotnetType;


    public override void SetArgument(Interval<decimal> argument)
    {
        _interval = argument;
    }
}
