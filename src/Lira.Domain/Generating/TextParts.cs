using System.Collections;

namespace Lira.Domain.Generating;

public class TextParts : IReadOnlyCollection<ITextPart>
{
    private readonly IReadOnlyCollection<ITextPart> _parts;

    public TextParts(IReadOnlyCollection<ITextPart> valueParts)
    {
        _parts = valueParts;
    }

    public int Count => _parts.Count;

    public IEnumerator<ITextPart> GetEnumerator()
    {
        return _parts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class TextPartsExtensions
{
    public static async Task<string> Generate(this IReadOnlyCollection<ITextPart> parts, RuleExecutingContext context)
    {
        var strs = new List<string>(parts.Count);

        foreach (var part in parts)
        {
            string? str = await part.Get(context);
            if(str != null)
                strs.Add(str);
        }

        return string.Concat(strs);
    }
}
