namespace Lira.Domain.Handling.Generating.Writers;

public class BodyGenerator(TextParts parts)
{
    internal async Task<IReadOnlyCollection<string>> Create(RuleExecutingContext context)
    {
        var result = new List<string>(parts.Count);
        foreach (var part in parts)
        {
            string? text = await part.Get(context);

            if (text != null)
                result.Add(text);
        }

        return result;
    }
}