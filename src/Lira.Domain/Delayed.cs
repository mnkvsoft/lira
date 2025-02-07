namespace Lira.Domain;

public delegate Task<TimeSpan> GetDelay(RuleExecutingContext context);

public record Delayed<T>(T Value, GetDelay? GetDelay);