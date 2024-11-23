using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.Custom.VariableModel;

public record VariableInfo(CustomItemName Name, ReturnType? Type);

public class MatchFunctionWithSaveVariable : IMatchFunction
{
    readonly IMatchFunction _matchFunction;
    readonly CustomItemName _variableName;
    readonly ReturnType _variableType;
    readonly IVariablesProvider _variablesProvider;

    public MatchFunctionRestriction Restriction { get; }

    public MatchFunctionWithSaveVariable(IMatchFunctionTyped matchFunction, VariableInfo variableInfo, IVariablesProvider variablesProvider)
    {
        _matchFunction = matchFunction;
        _variablesProvider = variablesProvider;
        Restriction = matchFunction.Restriction;
        _variableName = variableInfo.Name;
        _variableType = variableInfo.Type ?? matchFunction.ValueType;
    }

    public async Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        var isMatch = await _matchFunction.IsMatch(context, value);

        if (isMatch)
        {
            if (!TypedValueCreator.TryCreate(_variableType, value, out dynamic? variableValue, out var exc))
            {
                throw new Exception(
                    $"Can't cast value '{value}' " +
                    $"of type '{value?.GetType()}' " +
                    $"to type '{_variableType}'" +
                    $"for write to variable '{_variableName}'",
                    exc);
            }

            _variablesProvider.SetVariable(context, _variableName, variableValue);
        }

        return isMatch;
    }
}