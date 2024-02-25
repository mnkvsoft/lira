namespace Lira.Domain.TextPart;

public interface IObjectTextPart
{
    dynamic? Get(RuleExecutingContext context);
}
