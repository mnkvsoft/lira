using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel.DataImpls.Guid;

public class GuidData : Data<System.Guid>
{
    private readonly IReadOnlyDictionary<DataName, GuidDataRange> _ranges;

    public GuidData(DataName name, IReadOnlyDictionary<DataName, GuidDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<System.Guid>)p.Value), info)
    {
        _ranges = ranges;
    }

    public override IEnumerable<IStateful> GetStates() => _ranges.Select(p=> p.Value.GetState());
}
