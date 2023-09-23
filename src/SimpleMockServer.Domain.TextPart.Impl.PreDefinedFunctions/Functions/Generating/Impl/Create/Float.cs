using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create;

internal class Float : IObjectTextPart, IWithDecimalRangeArgumentFunction, IWithOptionalArgument
{
    public static string Name => "float";
    
    private Interval<decimal> _interval = new(0.01m, 10_000);
    
    public object Get(RequestData request) => Math.Round(Random.Shared.NextDecimal(_interval), 2);

    public void SetArgument(Interval<decimal> argument)
    {
        _interval = argument;
    }
}
