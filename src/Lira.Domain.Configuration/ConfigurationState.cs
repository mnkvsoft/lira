using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration;

public abstract record ConfigurationState
{
    public record Ok(
        DateTime LoadTime,
        IReadOnlyCollection<Rule> Rules,
        IReadOnlyCollection<IState> States) : ConfigurationState;

    public record Error(DateTime LoadTime, Exception Exception) : ConfigurationState;
}