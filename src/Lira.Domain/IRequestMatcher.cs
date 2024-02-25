namespace Lira.Domain;

internal interface IRequestMatcher
{
    // Task<RequestMatchResult> IsMatch(RequestData request);
}

public record RequestMatchResult
{
    public static readonly RequestMatchResult NotMatched = new NotMatched();
    public static RequestMatchResult Matched(int weight, IReadOnlyDictionary<string, string?> matchedValues) => new Matched(weight, matchedValues);
}

public record NotMatched : RequestMatchResult;
public record Matched(int Weight, IReadOnlyDictionary<string, string?> MatchedValues) : RequestMatchResult;

