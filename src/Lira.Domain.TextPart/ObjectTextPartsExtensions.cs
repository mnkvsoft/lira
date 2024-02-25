using Lira.Domain.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static dynamic? Generate(this IReadOnlyCollection<IObjectTextPart> parts, RuleExecutingContext context)
    {
        return parts.Count == 1 ? parts.First().Get(context) : string.Concat(parts.Select(p => GetStringValue(p.Get(context))));
    }

    public static TextParts WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextParts(parts.Select(x => new TextPartAdapter(x)).ToArray());
    }

    private record TextPartAdapter(IObjectTextPart ObjectTextPart) : ITextPart
    {
        public string? Get(RuleExecutingContext context) => GetStringValue(ObjectTextPart.Get(context));
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