using System.Collections.Immutable;
using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel.DataImpls.Hex;

public class HexData : Data<string>
{
    public HexData(DataName name, IReadOnlyDictionary<DataName, HexDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<string>)p.Value), info)
    {
    }

    public override IReadOnlySet<IStateful> GetStates()
    {
        return ImmutableHashSet<IStateful>.Empty;
    }
}
