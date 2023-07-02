namespace SimpleMockServer.Domain.DataModel.DataImpls.Float;

public class FloatData : Data<float>
{
    public FloatData(DataName name, IReadOnlyDictionary<DataName, DataRange<float>> ranges) :
        base(name, ranges.ToDictionary(x => x.Key, x => x.Value))
    {
    }
}
