using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Int;

public class IntData : Data<long>
{
    private readonly IReadOnlyDictionary<DataName, IntDataRange> _ranges;

    public IntData(DataName name, IReadOnlyDictionary<DataName, IntDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<long>)p.Value), info)
    {
        _ranges = ranges;
    }

    public override IEnumerable<IStateful> GetStates()
    {
        foreach (var range in _ranges)
        {
            var state = range.Value.GetStateful();
            if (state != null)
                yield return state;
        }
    }
}