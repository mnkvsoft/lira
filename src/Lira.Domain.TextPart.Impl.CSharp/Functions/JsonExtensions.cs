using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart.Impl.CSharp.Functions;

public static class JsonUtils
{
    public static Json json(string json)
    {
        return new Json(json);
    }
}