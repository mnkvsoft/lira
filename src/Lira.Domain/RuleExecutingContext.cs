using Lira.Domain.Extensions;

namespace Lira.Domain;

public record RuleExecutingContext(RequestContext RequestContext)
{
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    private Dictionary<string, string?>? _matchedValues;

    public void AddMatchedValue(IReadOnlyDictionary<string, string?> matchedValues)
    {
        _matchedValues ??= new Dictionary<string, string?>();
        _matchedValues.Add(matchedValues);
    }

    public string? GetValue(string id)
    {
        if(_matchedValues == null || !_matchedValues.TryGetValue(id, out var value))
            throw new InvalidOperationException($"Value with id = {id} not registered");
        return value;
    }
}