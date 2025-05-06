using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class MatchFunctionWithSaveVariable(IMatchFunctionTyped matchFunction, Variable variable) : IMatchFunction
{
    public MatchFunctionRestriction Restriction { get; } = matchFunction.Restriction;

    public async Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        var isMatch = await matchFunction.IsMatch(context, value);

        if (isMatch)
            variable.SetValue(context, value);

        return isMatch;
    }
}