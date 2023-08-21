namespace SimpleMockServer.Domain.DataModel.DataImpls.Float;

public class FloatData : Data<decimal>
{
    public FloatData(DataName name, IReadOnlyDictionary<DataName, DataRange<decimal>> ranges, string info) :
        base(name, ranges.ToDictionary(x => x.Key, x => x.Value), info)
    {
    }
}
