using System.Collections;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public class TextParts : IReadOnlyCollection<TextPart>
{
    private readonly IReadOnlyCollection<TextPart> _parts;

    public TextParts(IReadOnlyCollection<TextPart> valueParts)
    {
        _parts = valueParts;
    }

    public int Count => _parts.Count;

    public IEnumerator<TextPart> GetEnumerator()
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
    public static string Generate(this IReadOnlyCollection<TextPart> parts, RequestData request) => string.Concat(parts.Select(p => p.Get(request)));
}
