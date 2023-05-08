namespace SimpleMockServer.Domain.Configuration.Rules;

public abstract record ProviderState
{
    public record Ok : ProviderState;

    public record Error(Exception Exception) : ProviderState;
}