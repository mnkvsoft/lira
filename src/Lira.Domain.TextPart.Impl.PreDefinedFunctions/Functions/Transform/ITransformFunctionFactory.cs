using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Transform.Format;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Transform;

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

        result = null!;
        return false;
    }
}
