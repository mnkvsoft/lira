using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl;
using Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl.Format;

namespace Lira.Domain.TextPart.Impl.System.Functions.Transform;

class TransformFunctionFactory
{
    public bool TryCreate(string value, out ITransformFunction result)
    {
        if (value.StartsWith("format:"))
        {
            var (_, format) = value.SplitToTwoParts("format:").Trim();
            if (string.IsNullOrWhiteSpace(format))
                throw new Exception("Function 'format' argument required");

            result = new FormatFunction(format);
            return true;
        }

        if (value == "lower")
        {
            result = new LowerFunction();
            return true;
        }

        if (value == "upper")
        {
            result = new UpperFunction();
            return true;
        }

        result = null!;
        return false;
    }
}
