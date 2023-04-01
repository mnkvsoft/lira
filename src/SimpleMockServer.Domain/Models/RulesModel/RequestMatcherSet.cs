using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class RequestMatcherSet
{
    private readonly Dictionary<Type, IRequestMatcher> _matchers = new();

    public void AddRange(IEnumerable<IRequestMatcher> matchers)
    {
        foreach(var matcher in matchers)
        {
            Add(matcher);
        }
    }

    public void Add(IRequestMatcher matcher)
    {
        Type type = matcher.GetType();
        if (_matchers.ContainsKey(type))
            throw new InvalidOperationException($"Matcher '{type}' already added");

        _matchers.Add(type, matcher);   
    }

    public async Task<bool> IsMatch(HttpRequest request)
    {
        foreach (var matcher in _matchers.Values)
        {
            bool isMatch = await matcher.IsMatch(request);
            if (!isMatch)
            {
                return false;
            }
        }
        return true;
    }
}
