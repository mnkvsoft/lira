namespace Lira.Domain.TextPart;

public interface IObjectTextPart
{
    Task<dynamic?> Get(RuleExecutingContext context);
}