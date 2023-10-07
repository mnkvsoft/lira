namespace Lira.Domain;

public interface IRequestMatcher
{
    Task<RequestMatchResult> IsMatch(RequestData request);
}

public record RequestMatchResult
{
    public static readonly RequestMatchResult NotMatched = new NotMatched();
    public static RequestMatchResult Matched(int weight) => new Matched(weight);
}

public record NotMatched : RequestMatchResult;
public record Matched(int Weight) : RequestMatchResult;

