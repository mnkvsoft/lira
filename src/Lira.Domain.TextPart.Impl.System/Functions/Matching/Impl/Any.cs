using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Any : FunctionBase, IMatchFunctionSystem
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        // means that there is no such node in json when checking the body
        if (value == null)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}
