using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration;

static class ReturnTypeParser
{
    public static ReturnType Parse(string value)
    {
        return value switch
        {
            Consts.Type.Decimal => ReturnType.Decimal,
            Consts.Type.Json => ReturnType.Json,
            Consts.Type.Guid => ReturnType.Guid,
            Consts.Type.Int => ReturnType.Int,
            Consts.Type.String => ReturnType.String,
            _ => throw new Exception($"Unknown type: {value}")
        };
    }
}