using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

namespace Lira.Domain.Caching;

public abstract record CachingMode
{
    public record Enabled(TimeSpan Time, IRuleKeyExtractor RuleKeyExtractor) : CachingMode;

    public record Disabled : CachingMode
    {
        public static readonly Disabled Instance = new();
    }
}