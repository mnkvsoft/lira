namespace Lira.Domain;

public record RuleExecutingContext(RequestData RequestData)
{
    public IDictionary<Type, object> Items { get; } = new Dictionary<Type, object>();
}