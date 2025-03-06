namespace Lira.Domain.Handling.Generating;

public interface ITextPart
{
    Task<string?> Get(RuleExecutingContext context);
}
