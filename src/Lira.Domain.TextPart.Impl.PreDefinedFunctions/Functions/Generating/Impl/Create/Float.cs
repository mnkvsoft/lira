using Lira.Common;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Float : WithRangeArgumentFunction<decimal>, IObjectTextPart
{
    public static string Name => "float";
    public override bool ArgumentIsRequired => false;
    private Interval<decimal> _interval = new(0.01m, 10_000);

    public object Get(RequestData request) => Math.Round(Random.Shared.NextDecimal(_interval), 2, MidpointRounding.ToNegativeInfinity);


    public override void SetArgument(Interval<decimal> argument)
    {
        _interval = argument;
    }
}
