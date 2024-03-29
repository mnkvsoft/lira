using System.Diagnostics.CodeAnalysis;
using Lira.Common.Extensions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.CustomSetModel;
public class CustomSets
{
    private readonly Dictionary<string, CustomSetFunction> _map = new Dictionary<string, CustomSetFunction>();

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

public class CustomSetFunction : IObjectTextPart, IMatchFunction
{
    private readonly IReadOnlyList<string> _list;
    private readonly HashSet<string> _hashSet;

    public CustomSetFunction(IReadOnlyList<string> list)
    {
        _list = list;
        _hashSet = new HashSet<string>(list);
    }

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;

    public dynamic Get(RuleExecutingContext context)
    {
        return _list.Random();
    }

    public bool IsMatch(string? value)
    {
        if (value == null)
            return false;

        return _hashSet.Contains(value);
    }
}