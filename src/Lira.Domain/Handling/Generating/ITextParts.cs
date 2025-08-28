namespace Lira.Domain.Handling.Generating;

public interface ITextParts
{
    IEnumerable<string> Get(RuleExecutingContext context);
}
