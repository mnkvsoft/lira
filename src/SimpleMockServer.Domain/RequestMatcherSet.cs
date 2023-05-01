namespace SimpleMockServer.Domain;

public class RequestMatcherSet
{
    private readonly Dictionary<Type, IRequestMatcher> _matchers = new();

    public void AddRange(IEnumerable<IRequestMatcher> matchers)
    {
        foreach (var matcher in matchers)
        {
            Add(matcher);
        }
    }

    public void Add(IRequestMatcher matcher)
    {
        var type = matcher.GetType();
        if (_matchers.ContainsKey(type))
            throw new InvalidOperationException($"Matcher '{type}' already added");

        _matchers.Add(type, matcher);
    }

    public async Task<bool> IsMatch(RequestData request)
    {
        foreach (var matcher in _matchers.Values)
        {
            var isMatch = await matcher.IsMatch(request);
            if (!isMatch)
            {
                return false;
            }
        }
        return true;
    }
}
