using System.Text;

namespace Lira.Domain.Handling.Generating;

public class TextPartsProvider(IEnumerable<ITextParts> valueParts)
{
    public IEnumerable<string> Get(RuleExecutingContext ctx)
    {
        foreach (var part in valueParts)
        {
            foreach (var text in part.Get(ctx))
            {
                yield return text;
            }
        }
    }

    public string GetSingleString(RuleExecutingContext context)
    {
        var sb = new StringBuilder();

        foreach (var text in Get(context))
        {
            sb.Append(text);
        }

        return sb.ToString();
    }
}