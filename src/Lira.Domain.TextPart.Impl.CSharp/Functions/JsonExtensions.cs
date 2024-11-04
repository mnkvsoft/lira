using Lira.Domain.TextPart.Types;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lira.Domain.TextPart.Impl.CSharp.Functions;

public static class JsonUtils
{
    public static Json json(string json)
    {
        return new Json(json);
    }
}