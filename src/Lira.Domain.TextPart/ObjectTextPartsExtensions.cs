using Lira.Domain.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static async Task<dynamic?> Generate(this IReadOnlyCollection<IObjectTextPart> parts, RuleExecutingContext context)
    {
        return parts.Count == 1 ? await parts.First().Get(context) : string.Concat(parts.Select(async p => GetStringValue(await p.Get(context))));
    }

    public static TextParts WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextParts(parts.Select(x => new TextPartAdapter(x)).ToArray());
    }

    private record TextPartAdapter(IObjectTextPart ObjectTextPart) : ITextPart
    {
        public async Task<string?> Get(RuleExecutingContext context)
        {
            var bj = await ObjectTextPart.Get(context);
            return GetStringValue(bj);
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