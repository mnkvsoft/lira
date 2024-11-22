namespace Lira.Domain;

public interface IRequestMatcher
{
    Task<RequestMatchResult> IsMatch(RuleExecutingContext context);
}

public record RequestMatchResult
{
    public static readonly RequestMatchResult NotMatched = new NotMatched();
    public static RequestMatchResult Matched(string name, int weight) => new Matched(name, weight);
}

public record NotMatched : RequestMatchResult;
public record Matched(string Name, int Weight) : RequestMatchResult;

