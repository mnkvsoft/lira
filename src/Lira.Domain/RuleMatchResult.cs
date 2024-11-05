namespace Lira.Domain;

internal abstract record RuleMatchResult
{
    public record Matched(IRuleMatchWeight Weight) : RuleMatchResult;
    public record NotMatched : RuleMatchResult
    {
        public static readonly RuleMatchResult Instance = new NotMatched();
    }
}
