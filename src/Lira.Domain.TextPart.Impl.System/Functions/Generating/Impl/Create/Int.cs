using Lira.Common;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Long : WithRangeArgumentFunction<long>, IObjectTextPart
{
    public override string Name => "int";

    public override bool ArgumentIsRequired => false;
    
    private Interval<long> _range = new(1, int.MaxValue);

    public dynamic? Get(RuleExecutingContext context) => Random.Shared.NextInt64(_range);


    public override void SetArgument(Interval<long> argument)
    {
        _range = argument;
    }
}
