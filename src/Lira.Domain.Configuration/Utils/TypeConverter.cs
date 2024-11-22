using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Utils;

internal static class TypeConverter
{
    public static PartType Convert(string type)
    {
        return type switch
        {
            Consts.Type.Json => PartType.Json,
            Consts.Type.Guid => PartType.Guid,
            Consts.Type.String => PartType.String,
            Consts.Type.Int => PartType.Int,
            Consts.Type.Decimal => PartType.Decimal,
            _ => throw new ArgumentException($"Unknown type: {type}", nameof(type))
        };
    }
}