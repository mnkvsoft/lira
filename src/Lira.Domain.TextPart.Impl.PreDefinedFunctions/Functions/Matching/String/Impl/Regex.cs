using System.Text.RegularExpressions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Regex : WithArgumentFunction<string>, IMatchPrettyFunction
{
    public override string Name => "regex";
    public override bool ArgumentIsRequired => true;
    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;

    private System.Text.RegularExpressions.Regex _regex = null!;


    public bool IsMatch(string? value)
    {
        return _regex.IsMatch(value ?? "");
    }

    public override void SetArgument(string argument)
    {
        _regex = new System.Text.RegularExpressions.Regex(argument, RegexOptions.Compiled);
    }
}
