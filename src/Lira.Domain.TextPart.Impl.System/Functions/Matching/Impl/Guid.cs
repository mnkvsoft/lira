using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Guid : FunctionBase, IMatchFunctionTyped
{
    public override string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;
    public Type ValueType => DotNetType.Guid;

    public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

    public bool IsMatchTyped(RuleExecutingContext context, string? value, out dynamic? typedValue)
    {
        typedValue = null;

        if (global::System.Guid.TryParse(value, out var result))
        {
            typedValue = result;
            return true;
        }

        return false;
    }
}
