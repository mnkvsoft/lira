using ArgValidation;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid;

public class GuidData : Data<System.Guid>
{
    public GuidData(DataName name, IReadOnlyDictionary<DataName, GuidDataRange> ranges)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<System.Guid>)p.Value))
    { 
    }
}
