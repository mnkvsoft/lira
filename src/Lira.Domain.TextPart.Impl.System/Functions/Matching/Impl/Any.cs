using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Any : FunctionBase, IMatchFunctionTyped
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public ReturnType ValueType => ReturnType.String;

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        // means that there is no such node in json when checking the body
        if (value == null)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}
