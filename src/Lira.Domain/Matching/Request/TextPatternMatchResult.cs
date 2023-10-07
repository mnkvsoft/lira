namespace Lira.Domain.Matching.Request;

public abstract record TextPatternMatchResult
{
    public record Matched(int Height) : TextPatternMatchResult;
    public record NotMatched : TextPatternMatchResult;
}
