using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Any : FunctionBase, IMatchFunctionTyped
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public Type ValueType => DotNetType.String;

    public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = null;
        if (value == null)
            return false;

        typedValue = value;
        return true;
    }
}