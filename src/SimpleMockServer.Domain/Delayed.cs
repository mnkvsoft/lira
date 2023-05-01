namespace SimpleMockServer.Domain;

public record Delayed<T>(T Value, TimeSpan? Delay);
