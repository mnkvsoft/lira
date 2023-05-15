using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions;

internal interface IWithRangeArgumentFunction : IWithArgument
{
    void SetArgument(Interval<long> argument);
}
