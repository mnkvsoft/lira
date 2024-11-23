using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Utils;

internal static class TypeConverter
{
    public static ReturnType Convert(string type)
    {
        return type switch
        {
            Consts.Type.Json => ReturnType.Json,
            Consts.Type.Guid => ReturnType.Guid,
            Consts.Type.String => ReturnType.String,
            Consts.Type.Int => ReturnType.Int,
            Consts.Type.Decimal => ReturnType.Decimal,
            _ => throw new ArgumentException($"Unknown type: {type}", nameof(type))
        };
    }
}