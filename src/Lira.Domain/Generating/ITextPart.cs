namespace Lira.Domain.Generating;

public interface ITextPart
{
    Task<string?> Get(RuleExecutingContext context);
}
