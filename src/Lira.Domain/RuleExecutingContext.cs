namespace Lira.Domain;

public record RuleExecutingContext(RequestContext RequestContext)
{
    public IDictionary<Type, object> Items { get; } = new Dictionary<Type, object>();
}