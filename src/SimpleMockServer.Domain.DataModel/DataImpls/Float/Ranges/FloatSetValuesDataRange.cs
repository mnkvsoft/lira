namespace SimpleMockServer.Domain.DataModel.DataImpls.Float.Ranges;

public class FloatSetValuesDataRange : FloatDataRange
{
    public IReadOnlyList<float> Values { get; }

    public FloatSetValuesDataRange(DataName name, IReadOnlyList<float> values) : base(name)
    {
        Values = values;
    }

    public override float Next()
    {
        var index = Random.Shared.Next(0, Values.Count);
        return Values[index];
    }

    public override bool IsBelong(float value)
    {
        return Values.Contains(value);
    }
}
