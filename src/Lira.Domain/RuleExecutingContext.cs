namespace Lira.Domain;

public record RuleExecutingContext(RequestData Request, IReadOnlyDictionary<string, string?> MatchedValues)
{
    public IDictionary<Type, object> Items { get; } = new Dictionary<Type, object>();

    public string? GetValue(string id)
    {
        if (!MatchedValues.TryGetValue(id, out var value))
            throw new InvalidOperationException($"Value with id = {id} not registered");
        return value;
    }
}