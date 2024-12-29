using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Dec;

public class DecData : Data<decimal>
{
    public DecData(DataName name, IReadOnlyDictionary<DataName, DataRange<decimal>> ranges, string info) :
        base(name, ranges.ToDictionary(x => x.Key, x => x.Value), info)
    {
    }

    public override IEnumerable<IStateful> GetStates() => Array.Empty<IStateful>();
}
