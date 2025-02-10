using Lira.Domain.Handling.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static async Task<dynamic?> Generate(this IReadOnlyCollection<IObjectTextPart> parts, RuleExecutingContext context)
    {
        if (parts.Count == 1)
        {
            var first = parts.First();
            dynamic? o = await first.Get(context);
            return o;
        }

        var results = new List<string>(parts.Count);
        foreach (var part in parts)
        {
            var obj = await part.Get(context);
            var str = GetStringValue(obj);
            if (str != null)
                results.Add(str);
        }

        return string.Concat(results);
    }

    public static TextParts WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextParts(parts.Select(x => new TextPartAdapter(x)).ToArray());
    }

    private record TextPartAdapter(IObjectTextPart ObjectTextPart) : ITextPart
    {
        public async Task<string?> Get(RuleExecutingContext context)
        {
            return GetStringValue(await ObjectTextPart.Get(context));
        }
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