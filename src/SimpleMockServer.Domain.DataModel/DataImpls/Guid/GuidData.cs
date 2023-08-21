namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid;

public class GuidData : Data<System.Guid>
{
    public GuidData(DataName name, IReadOnlyDictionary<DataName, GuidDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<System.Guid>)p.Value), info)
    { 
    }
}
