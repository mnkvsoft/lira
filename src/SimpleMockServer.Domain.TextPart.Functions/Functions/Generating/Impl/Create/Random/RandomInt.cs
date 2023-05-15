using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Create.Random;

internal class RandomInt : IGlobalObjectTextPart, IWithRangeArgumentFunction, IWithOptionalArgument
{
    public static string Name => "random.int";
    
    private readonly System.Random Random = new();
    
    private Interval<long> _range = new(1, int.MaxValue);
    
    public object Get(RequestData request) => Get();

    public object Get() => Random.NextInt64(_range.From, _range.To);

    public void SetArgument(Interval<long> argument)
    {
        _range = argument;
    }
}
