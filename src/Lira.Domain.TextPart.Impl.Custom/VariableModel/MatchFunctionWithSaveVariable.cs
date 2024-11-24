using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public class MatchFunctionWithSaveVariable : IMatchFunction
{
    readonly IMatchFunction _matchFunction;
    readonly Variable _variable;

    public MatchFunctionRestriction Restriction { get; }

    public MatchFunctionWithSaveVariable(IMatchFunctionTyped matchFunction, Variable variable)
    {
        _matchFunction = matchFunction;
        _variable = variable;
        Restriction = matchFunction.Restriction;
    }

    public async Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        var isMatch = await _matchFunction.IsMatch(context, value);

        if (isMatch)
            _variable.SetValue(context, value);

        return isMatch;
    }
}