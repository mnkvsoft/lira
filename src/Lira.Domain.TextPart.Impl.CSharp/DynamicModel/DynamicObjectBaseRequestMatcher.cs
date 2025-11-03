// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using Lira.Domain.Matching.Request;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public abstract class DynamicObjectBaseRequestMatcher : DynamicObjectWithDeclaredPartsBase, IRequestMatcher
{
    protected IReadonlyCache cache => Cache;

    protected DynamicObjectBaseRequestMatcher(DependenciesBase dependencies) : base(dependencies)
    {
    }

    public Task<RequestMatchResult> IsMatch(RuleExecutingContext ctx)
    {
        bool isMatch = IsMatchInternal(ctx);
        return Task.FromResult(isMatch
                                ? RequestMatchResult.Matched(name: "custom_code", WeightValue.CustomCode)
                                : RequestMatchResult.NotMatched);
    }

    protected abstract bool IsMatchInternal(RuleExecutingContext ctx);
}
