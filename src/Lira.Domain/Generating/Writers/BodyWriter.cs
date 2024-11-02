namespace Lira.Domain.Generating.Writers;

public class BodyGenerator
{
    private readonly TextParts _parts;

    public BodyGenerator(TextParts parts)
    {
        _parts = parts;
    }

    internal IEnumerable<string> Create(RuleExecutingContext context)
    {
        foreach (var part in _parts)
        {
            string? text = part.Get(context);
            
            if (text != null)
                yield return text;
        }
    }
}