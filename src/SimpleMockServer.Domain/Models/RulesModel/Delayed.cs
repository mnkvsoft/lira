namespace SimpleMockServer.Domain.Models.RulesModel;

public record Delayed<T>(T Value, TimeSpan? Delay);
