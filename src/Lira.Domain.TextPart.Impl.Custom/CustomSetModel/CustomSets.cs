using Lira.Common.Extensions;

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

    public IObjectTextPart? TryGetCustomSetFunction(string name)
    {
        if(_map.TryGetValue(name, out var function))
            return function;
        return null;
    }

    class CustomSetFunction : IObjectTextPart
    {
        private readonly IReadOnlyList<string> _list;

        public CustomSetFunction(IReadOnlyList<string> list)
        {
            _list = list;
        }

        public dynamic Get(RequestData request)
        {
            return _list.Random();
        }
    }
}
