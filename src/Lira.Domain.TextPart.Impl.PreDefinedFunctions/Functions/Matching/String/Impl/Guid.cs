using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Guid : FunctionBase, IMatchPrettyFunction
{
    
    public override string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;

    public bool IsMatch(string? value)
    {
        return global::System.Guid.TryParse(value, out _);
    }
}
