using System.Collections;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class RequestMatcherSet : IEnumerable<IRequestMatcher>
{
    private readonly Dictionary<Type, IRequestMatcher> _matchers = new Dictionary<Type, IRequestMatcher>();



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

    public IEnumerator<IRequestMatcher> GetEnumerator()
    {
        return _matchers.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
