using Lira.Common.State;

namespace Lira.Domain.Configuration;

public abstract record ConfigurationState
{
    public record Ok(
        DateTime LoadTime,
        IReadOnlyCollection<Rule> Rules,
        IReadOnlyCollection<IStateful> Statefuls) : ConfigurationState;

    public record Error(DateTime LoadTime, Exception Exception) : ConfigurationState;
}