namespace SimpleMockServer.Domain.DataModel.DataImpls.Number.Ranges;

public class NumberSetValuesDataRange : NumberDataRange
{
    public IReadOnlyList<long> Values { get; }

    public NumberSetValuesDataRange(DataName name, IReadOnlyList<long> values) : base(name)
    {
        Values = values;
    }

    public override long Next()
    {
        var index = Random.Shared.Next(0, Values.Count);
        return Values[index];
    }

    public override bool IsBelong(long value)
    {
        return Values.Contains(value);
    }
}
