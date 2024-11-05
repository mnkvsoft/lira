namespace Lira.Domain;

public interface IRequestMatcher
{
    Task<RequestMatchResult> IsMatch(IRuleExecutingContextReadonly context);
}

public record RequestMatchResult
{
    public static readonly RequestMatchResult NotMatched = new NotMatched();
    public static RequestMatchResult Matched(string name, int weight, IReadOnlyDictionary<string, string?> matchedValues) => new Matched(name, weight, matchedValues);
}

public record NotMatched : RequestMatchResult;
public record Matched(string Name, int Weight, IReadOnlyDictionary<string, string?> MatchedValues) : RequestMatchResult;

