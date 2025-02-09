namespace Lira.Domain.Handling.Generating.Writers;

public class BodyGenerator
{
    private readonly TextParts _parts;

    public BodyGenerator(TextParts parts)
    {
        _parts = parts;
    }

    internal async Task<IReadOnlyCollection<string>> Create(RuleExecutingContext context)
    {
        var result = new List<string>(_parts.Count);
        foreach (var part in _parts)
        {
            string? text = await part.Get(context);

            if (text != null)
                result.Add(text);
        }

        return result;
    }
}