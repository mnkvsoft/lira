using System.Text.RegularExpressions;
using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Matching.String.Impl;

internal class Regex : IStringMatchPrettyFunction, IWithStringArgumentFunction
{
    public static string Name => "regex";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;

    private global::System.Text.RegularExpressions.Regex _regex = null!;
    
    public bool IsMatch(string? value)
    {
        return _regex.IsMatch(value ?? "");
    }

    public void SetArgument(string argument)
    {
        _regex = new global::System.Text.RegularExpressions.Regex(argument, RegexOptions.Compiled);
    }
}
