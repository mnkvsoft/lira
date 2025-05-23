using System.Text.RegularExpressions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Regex : WithArgumentFunction<string>, IMatchFunctionTyped
{
    public override string Name => "regex";
    public override bool ArgumentIsRequired => true;
    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
    public ReturnType ValueType => ReturnType.String;

    private global::System.Text.RegularExpressions.Regex _regex = null!;


    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        return Task.FromResult(_regex.IsMatch(value ?? ""));
    }

    public override void SetArgument(string arguments)
    {
        _regex = new global::System.Text.RegularExpressions.Regex(arguments, RegexOptions.Compiled);
    }
}
