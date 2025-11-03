namespace Lira.Domain.Handling.Generating;

public class TextPartsProvider(IEnumerable<ITextParts> parts)
{
    public IEnumerable<string> Get(RuleExecutingContext ctx)
    {
        foreach (var part in parts)
        {
            foreach (var text in part.Get(ctx))
            {
                yield return text;
            }
        }
    }

    public string GetSingleString(RuleExecutingContext context) => string.Concat(Get(context));
}