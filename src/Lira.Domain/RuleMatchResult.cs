namespace Lira.Domain;

public abstract record RuleMatchResult
{
    public static readonly RuleMatchResult NotMatchedInstance = new NotMatched();
    public record Matched(IRuleMatchWeight Weight) : RuleMatchResult;
    public record NotMatched : RuleMatchResult;
}
