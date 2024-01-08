namespace Lira.Domain.Generating.Writers;

public class BodyGenerator
{
    private readonly TextParts _parts;

    public BodyGenerator(TextParts parts)
    {
        _parts = parts;
    }

    internal IReadOnlyCollection<string> Create(RequestData request)
    {
        var result = new List<string>(_parts.Count);
        foreach (var part in _parts)
        {
            string? text = part.Get(request);
            
            if (text != null)
                result.Add(text);
        }

        return result;
    }
}