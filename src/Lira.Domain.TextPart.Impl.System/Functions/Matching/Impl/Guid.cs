using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.System.Functions.Matching.Impl;

internal class Guid : FunctionBase, IMatchFunctionSystem
{

    public override string Name => "guid";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;

    public Task<bool> IsMatch(RuleExecutingContext context, string? value)
    {
        return Task.FromResult(global::System.Guid.TryParse(value, out _));
    }
}
