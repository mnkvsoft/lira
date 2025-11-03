using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Guid : FunctionBase, IMatchFunctionTyped
{
    public override string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;
    public ReturnType ValueType => ReturnType.Guid;

    public bool IsMatch(RuleExecutingContext context, string? value)
    {
        return global::System.Guid.TryParse(value, out _);
    }
}
