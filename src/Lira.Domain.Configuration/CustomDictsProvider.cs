using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration;

class CustomDictsProvider : ICustomDictsProvider
{
    private IReadOnlyDictionary<string, CustomDic>? _dicts = null;

    internal IReadOnlyDictionary<string, CustomDic> Dicts
    {
        get
        {
            if (_dicts == null)
                throw new InvalidOperationException("Custom dictionaries are not initialized");
            return _dicts;
        }
        set
        {
            _dicts = value;
        }
    }

    public CustomDic GetCustomDic(string name)
    {
        if(!Dicts.TryGetValue(name, out var dic))
            throw new InvalidOperationException($"Custom dic function '{name}' is not registered. Registered: {string.Join(", ", Dicts.Keys)}");
        return dic;
    }
}