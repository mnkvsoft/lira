namespace Lira.Domain.Generating;

public interface ITextPart
{
    string? Get(RuleExecutingContext context);
}
