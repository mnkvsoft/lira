using System.Collections.Immutable;
using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel.DataImpls.Float;

public class FloatData : Data<decimal>
{
    public FloatData(DataName name, IReadOnlyDictionary<DataName, DataRange<decimal>> ranges, string info) :
        base(name, ranges.ToDictionary(x => x.Key, x => x.Value), info)
    {
    }

    public override IEnumerable<IStateful> GetStates() => Array.Empty<IStateful>();
}
