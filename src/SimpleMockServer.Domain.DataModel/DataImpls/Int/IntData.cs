namespace SimpleMockServer.Domain.DataModel.DataImpls.Int;

public class IntData : Data<long>
{
    public IntData(DataName name, IReadOnlyDictionary<DataName, DataRange<long>> ranges, string info) : base(name, ranges, info)
    {
    }
}
