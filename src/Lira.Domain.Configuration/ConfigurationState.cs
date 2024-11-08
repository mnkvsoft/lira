using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Impl.System;

namespace Lira.Domain.Configuration;

public abstract record ConfigurationState
{
    public record Ok(
        DateTime LoadTime,
        IReadOnlyCollection<Rule> Rules,
        IReadOnlyDictionary<DataName, Data> Ranges,
        Sequence Sequence) : ConfigurationState;

    public record Error(DateTime LoadTime, Exception Exception) : ConfigurationState;
}