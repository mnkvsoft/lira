namespace Lira.Domain.Handling.Generating;

public interface ITextParts
{
    IAsyncEnumerable<string> Get(RuleExecutingContext context);
}
