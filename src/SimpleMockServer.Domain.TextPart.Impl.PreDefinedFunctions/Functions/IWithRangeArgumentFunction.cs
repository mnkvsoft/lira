using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

internal interface IWithLongRangeArgumentFunction : IWithArgument
{
    void SetArgument(Interval<long> argument);
}

internal interface IWithDecimalRangeArgumentFunction : IWithArgument
{
    void SetArgument(Interval<decimal> argument);
}
