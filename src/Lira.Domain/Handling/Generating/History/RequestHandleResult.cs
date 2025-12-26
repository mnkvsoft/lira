namespace Lira.Domain.Handling.Generating.History;

abstract record RequestHandleResult
{
    public record Response(int StatusCode, IReadOnlyCollection<Header>? Headers, string? Body) : RequestHandleResult;

    public record Fault : RequestHandleResult
    {
        public static readonly Fault Instance = new();
    }
}