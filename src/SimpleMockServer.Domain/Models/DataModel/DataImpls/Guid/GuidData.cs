namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Guid;

class GuidData : Data<System.Guid>
{
    public GuidData(DataName name, IReadOnlyDictionary<DataName, GuidDataRange> ranges)
        : base(name, ranges.ToDictionary(x => x.Key, x => (DataRange<System.Guid>)x.Value))
    {
    }
}
