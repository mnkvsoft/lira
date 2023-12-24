using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Any : FunctionBase, IMatchFunctionSystem
{
    public override string Name => "any";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Any;

    public bool IsMatch(string? value)
    {
        return true;
    }
}
