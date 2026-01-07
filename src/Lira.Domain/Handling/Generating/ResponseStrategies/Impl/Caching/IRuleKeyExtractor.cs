namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

public interface IRuleKeyExtractor
{
    string Extract(RuleExecutingContext ruleExecutingContext);
}