namespace Lira.Domain;

public delegate TimeSpan GetDelay(RuleExecutingContext context);

public record Delayed<T>(T Value, GetDelay? GetDelay);