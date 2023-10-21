using Lira.Common;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Long : WithRangeArgumentFunction<long>, IObjectTextPart
{
    public static string Name => "int";

    public override bool ArgumentIsRequired => false;
    
    private Interval<long> _range = new(1, int.MaxValue);

    public object Get(RequestData request) => Random.Shared.NextInt64(_range.From, _range.To);


    public override void SetArgument(Interval<long> argument)
    {
        _range = argument;
    }
}
