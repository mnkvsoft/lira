using System.Text.RegularExpressions;
using SimpleMockServer.Domain.Matching.Matchers.Body;

namespace SimpleMockServer.Domain.Matching.Matchers.Body.Patterns;

class RegexBodyPattern : IBodyPattern
{
    private readonly Regex _regex;

    public RegexBodyPattern(string regex)
    {
        _regex = new Regex(regex, RegexOptions.Compiled);
    }

    public bool IsMatch(string body)
    {
        return _regex.IsMatch(body);
    }
}