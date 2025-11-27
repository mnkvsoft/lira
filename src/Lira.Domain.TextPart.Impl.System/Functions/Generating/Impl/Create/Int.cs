using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class Int : WithRangeArgumentFunction<long>, IObjectTextPart
{
    public override string Name => "int";

    public override bool ArgumentIsRequired => false;

    private Interval<long> _range = new(1, int.MaxValue);

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return Random.Shared.NextInt64(_range);
    }

    public ReturnType ReturnType => ReturnType.Int;


    public override void SetArgument(Interval<long> argument)
    {
        _range = argument;
    }
}
