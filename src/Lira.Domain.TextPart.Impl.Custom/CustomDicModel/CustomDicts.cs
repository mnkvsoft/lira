using System.Diagnostics.CodeAnalysis;
using Lira.Common.Extensions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

public interface IReadOnlyCustomDicts
{
    bool TryGetCustomSetFunction(string name, [MaybeNullWhen(false)] out CustomSetFunction function);
    IReadOnlyCollection<string> GetRegisteredNames();
}

public class CustomDicts : IReadOnlyCustomDicts
{
    private readonly Dictionary<string, CustomSetFunction> _map;

    public CustomDicts(IReadOnlyCustomDicts customDicts)
    {
        _map = ((CustomDicts)customDicts)._map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public CustomDicts()
    {
        _map = new Dictionary<string, CustomSetFunction>();
    }

    public void Add(string name, IReadOnlyList<string> lines)
    {
        if (_map.ContainsKey(name))
            throw new InvalidOperationException($"Set '{name}' already register");

        _map.Add(name, new CustomSetFunction(lines));
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
    {
        return _map.Keys;
    }

    public bool TryGetCustomSetFunction(string name, [MaybeNullWhen(false)] out CustomSetFunction function)
    {
        return _map.TryGetValue(name, out function);
    }
}

public class CustomSetFunction : IObjectTextPart, IMatchFunctionTyped
{
    private readonly IReadOnlyList<string> _list;
    private readonly HashSet<string> _hashSet;

    public CustomSetFunction(IReadOnlyList<string> list)
    {
        _list = list;
        _hashSet = new HashSet<string>(list);
    }

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    public ReturnType ValueType => ReturnType.String;

    public Task<dynamic?> Get(RuleExecutingContext context)
    {
        return Task.FromResult<dynamic?>(_list.Random());
    }

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        if (value == null)
            return Task.FromResult(false);

        return Task.FromResult(_hashSet.Contains(value));
    }
}