// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System.Collections.Immutable;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public abstract class DynamicObjectBaseRequestMatcher : DynamicObjectBaseMatch, IRequestMatcher
{
    protected DynamicObjectBaseRequestMatcher(DependenciesBase dependencies) : base(dependencies)
    {
    }

    public async Task<RequestMatchResult> IsMatch(IRuleExecutingContextReadonly ctx)
    {
        bool isMatch = await IsMatchInternal(ctx);
        return isMatch
            ? RequestMatchResult.Matched(name: "custom", 100, ImmutableDictionary<string, string?>.Empty)
            : RequestMatchResult.NotMatched;
    }

    protected abstract Task<bool> IsMatchInternal(IRuleExecutingContextReadonly ctx);
}
