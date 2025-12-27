namespace Lira.Domain.Handling.Generating.History;

public abstract record RequestHandleResult
{
    public record Response(int StatusCode, IReadOnlyDictionary<string, string?>? Headers, string? Body) : RequestHandleResult;

    public record Fault : RequestHandleResult
    {
        public static readonly Fault Instance = new();
    }
}