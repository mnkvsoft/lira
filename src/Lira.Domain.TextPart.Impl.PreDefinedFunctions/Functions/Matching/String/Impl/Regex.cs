using System.Text.RegularExpressions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Regex : WithArgumentFunction<string>, IStringMatchPrettyFunction
{
    public static string Name => "regex";
    public override bool ArgumentIsRequired => true;
    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Range;

    private global::System.Text.RegularExpressions.Regex _regex = null!;
    
    public bool IsMatch(string? value)
    {
        return _regex.IsMatch(value ?? "");
    }

    public override void SetArgument(string argument)
    {
        _regex = new global::System.Text.RegularExpressions.Regex(argument, RegexOptions.Compiled);
    }
}
