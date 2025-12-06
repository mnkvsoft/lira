namespace Lira.Domain;

internal abstract record RuleMatchResult
{
    public record Matched(RuleMatchWeight Weight, Rule Rule) : RuleMatchResult;
    public record NotMatched : RuleMatchResult
    {
        public static readonly RuleMatchResult Instance = new NotMatched();
    }
}
