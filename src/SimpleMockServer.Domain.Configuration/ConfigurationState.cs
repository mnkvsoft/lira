namespace SimpleMockServer.Domain.Configuration;

public abstract record ConfigurationState
{
    public record Ok : ConfigurationState;

    public record Error(Exception Exception) : ConfigurationState;
}
