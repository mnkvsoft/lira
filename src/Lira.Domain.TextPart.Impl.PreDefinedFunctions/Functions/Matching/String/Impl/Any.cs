using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Any : FunctionBase, IMatchPrettyFunction
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public bool IsMatch(string? value)
    {
        return true;
    }
}
