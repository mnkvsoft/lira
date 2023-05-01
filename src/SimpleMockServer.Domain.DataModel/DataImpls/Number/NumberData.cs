namespace SimpleMockServer.Domain.DataModel.DataImpls.Number;

public class NumberData : Data<long>
{
    public NumberData(DataName name, IReadOnlyDictionary<DataName, NumberDataRange> ranges) :
        base(name, ranges.ToDictionary(x => x.Key, x => (DataRange<long>)x.Value))
    {
    }
}
