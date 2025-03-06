using System.Collections.Immutable;
using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Pan;

public class PanData : Data<string>
{
    public PanData(DataName name, IReadOnlyDictionary<DataName, PanDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<string>)p.Value), info)
    {
    }

    public override IReadOnlySet<IStateful> GetStates()
    {
        return ImmutableHashSet<IStateful>.Empty;
    }
}
