using Lira.Domain.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static dynamic? Generate(this IReadOnlyCollection<IObjectTextPart> parts, RequestData request)
    {
        return parts.Count == 1 ? parts.First().Get(request) : string.Concat(parts.Select(p => GetStringValue(p.Get(request))));
    }

    public static TextParts WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextParts(parts.Select(x => new TextPartAdapter(x)).ToArray());
    }

    private record TextPartAdapter(IObjectTextPart ObjectTextPart) : ITextPart
    {
        public string? Get(RequestData request) => GetStringValue(ObjectTextPart.Get(request));
    }

    private static string? GetStringValue(dynamic? obj)
    {
        if (obj == null)
            return null;

        if (obj is DateTime date)
            return date.ToString("O");

        return obj.ToString();
    }
}