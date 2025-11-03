using System.Diagnostics.CodeAnalysis;
using Lira.Common.Extensions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

public interface IReadOnlyCustomDicts
{
    bool TryGetCustomDicFunction(string name, [MaybeNullWhen(false)] out CustomDicFunction function);
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

    public bool TryGetCustomDicFunction(string name, [MaybeNullWhen(false)] out CustomDicFunction function)
    {
        return _map.TryGetValue(name, out function);
    }
}

public class CustomDicFunction(IReadOnlyList<string> list) : IObjectTextPart, IMatchFunctionTyped
{
    private readonly HashSet<string> _hashSet = new(list);

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;
    Type IMatchFunctionTyped.ValueType => DotNetType.String;
    public Type Type => typeof(string);

    public dynamic Get(RuleExecutingContext context)
    {
        return list.Random();
    }

    public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = false;

        if (value == null)
            return false;

        if (_hashSet.Contains(value))
        {
            typedValue = value;
            return true;
        }

        return false;
    }
}