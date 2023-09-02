using System.Diagnostics.CodeAnalysis;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.TextPart.System.Functions.Functions.Transform.Format;

namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Transform;

public interface ITransformFunctionFactory
{
    bool TryCreate(string value, [MaybeNullWhen(false)] out ITransformFunction result);
}

class TransformFunctionFactory : ITransformFunctionFactory
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
