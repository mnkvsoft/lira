using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.Impl;

internal class Guid : FunctionBase, IMatchFunctionPreDefined
{

    public override string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;

    public bool IsMatch(string? value)
    {
        return System.Guid.TryParse(value, out _);
    }
}
