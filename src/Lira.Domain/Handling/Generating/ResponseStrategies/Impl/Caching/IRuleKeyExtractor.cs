namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Caching;

public interface IRuleKeyExtractor
{
    string? Extract(RuleExecutingContext ruleExecutingContext);
}

public class RuleKeyExtractor(TextPartsProvider provider) : IRuleKeyExtractor
{
    public string Extract(RuleExecutingContext ruleExecutingContext)
    {
        var ruleKey = provider.GetSingleString(ruleExecutingContext);

        if(string.IsNullOrWhiteSpace(ruleKey))
            throw new Exception("Rule key is empty");

        return ruleKey;
    }
}