using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

public class MatchFunctionWithSaveVariable(IMatchFunctionTyped matchFunction, RuleVariable ruleVariable) : IMatchFunction
{
    public MatchFunctionRestriction Restriction { get; } = matchFunction.Restriction;

    public bool IsMatch(RuleExecutingContext context, string? value)
    {
        var isMatch = matchFunction.IsMatchTyped(context, value, out dynamic? typedValue);

        if (isMatch)
            ruleVariable.SetValue(context, typedValue);

        return isMatch;
    }
}