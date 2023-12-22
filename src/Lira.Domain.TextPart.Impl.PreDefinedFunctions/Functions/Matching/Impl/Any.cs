using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Matching.Impl;

internal class Any : FunctionBase, IMatchFunctionPreDefined
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public bool IsMatch(string? value)
    {
        return true;
    }
}
