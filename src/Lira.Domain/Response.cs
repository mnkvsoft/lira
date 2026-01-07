namespace Lira.Domain;

public abstract record Response
{
    public record Normal(int Code, IReadOnlyDictionary<string, string?>? Headers, string? Body) : Response;

    public record Fault : Response
    {
        public static readonly Fault Instance = new();
    }
}