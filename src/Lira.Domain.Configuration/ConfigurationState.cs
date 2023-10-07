namespace Lira.Domain.Configuration;

public abstract record ConfigurationState
{
    public record Ok(DateTime LoadTime) : ConfigurationState;

    public record Error(DateTime LoadTime, Exception Exception) : ConfigurationState;
}
