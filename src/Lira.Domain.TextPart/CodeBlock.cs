namespace Lira.Domain.TextPart;

public class CodeBlock
{
    public readonly IReadOnlyCollection<CodeToken> Tokens;

    public CodeBlock(IReadOnlyCollection<CodeToken> tokens)
    {
        Tokens = tokens;
    }

    public override string ToString()
    {
        return string.Concat(Tokens.Select(x => x.RawValue));
    }
}