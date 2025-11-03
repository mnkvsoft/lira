using System.Text.RegularExpressions;
using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Regex : WithArgumentFunction<string>, IMatchFunctionTyped
{
    public override string Name => "regex";
    public override bool ArgumentIsRequired => true;
    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
    public Type ValueType => DotNetType.String;

    private global::System.Text.RegularExpressions.Regex _regex = null!;

   public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = null;

        if(_regex.IsMatch(value ?? ""))
        {
            typedValue = value;
            return true;
        }

        return false;
    }

    public override void SetArgument(string arguments)
    {
        _regex = new global::System.Text.RegularExpressions.Regex(arguments, RegexOptions.Compiled);
    }
}
