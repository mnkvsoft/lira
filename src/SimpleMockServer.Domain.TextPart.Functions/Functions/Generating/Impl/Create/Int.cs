using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create;

internal class Long : IGlobalObjectTextPart, IWithLongRangeArgumentFunction, IWithOptionalArgument
{
    public static string Name => "int";
    
    private Interval<long> _range = new(1, int.MaxValue);
    
    public object Get(RequestData request) => Get();

    public object Get() => Random.Shared.NextInt64(_range.From, _range.To);

    public void SetArgument(Interval<long> argument)
    {
        _range = argument;
    }
}
