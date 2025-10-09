using System.Diagnostics.CodeAnalysis;
using Lira.Common.Extensions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

public interface IReadOnlyCustomDicts
{
    bool TryGetCustomSetFunction(string name, [MaybeNullWhen(false)] out CustomDicFunction function);
    IReadOnlyCollection<string> GetRegisteredNames();
}

public class CustomDicts : IReadOnlyCustomDicts
{
    private readonly Dictionary<string, CustomDicFunction> _map;

    public CustomDicts(IReadOnlyCustomDicts customDicts)
    {
        _map = ((CustomDicts)customDicts)._map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public CustomDicts()
    {
        _map = new Dictionary<string, CustomDicFunction>();
    }

    public void Add(string name, IReadOnlyList<string> lines)
    {
        if (_map.ContainsKey(name))
            throw new InvalidOperationException($"Dictionary '{name}' already register");

        _map.Add(name, new CustomDicFunction(lines));
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
    {
        return _map.Keys;
    }

    public bool TryGetCustomSetFunction(string name, [MaybeNullWhen(false)] out CustomDicFunction function)
    {
        return _map.TryGetValue(name, out function);
    }
}

public class CustomDicFunction(IReadOnlyList<string> list) : IObjectTextPart, IMatchFunctionTyped
{
    private readonly HashSet<string> _hashSet = new(list);

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    ReturnType IMatchFunctionTyped.ValueType => ReturnType.String;
    ReturnType IObjectTextPart.ReturnType => ReturnType.String;

    public IEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        yield return list.Random();
    }

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        if (value == null)
            return Task.FromResult(false);

        return Task.FromResult(_hashSet.Contains(value));
    }
}