using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Float : IGlobalObjectTextPart, IWithDecimalRangeArgumentFunction, IWithOptionalArgument
{
    public static string Name => "float";
    
    private Interval<decimal> _interval = new(0.01m, 10_000);
    
    public object Get(RequestData request) => Get();

    public object Get() => Math.Round(Random.Shared.NextDecimal(_interval), 2);

    public void SetArgument(Interval<decimal> argument)
    {
        _interval = argument;
    }
}
