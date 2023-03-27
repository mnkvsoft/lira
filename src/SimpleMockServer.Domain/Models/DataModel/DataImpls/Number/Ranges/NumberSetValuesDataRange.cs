namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Number.Ranges;

class NumberSetValuesDataRange : NumberDataRange
{
    public IReadOnlyList<long> Values { get; }

    public NumberSetValuesDataRange(DataName name, IReadOnlyList<long> values) : base(name)
    {
        Values = values;
    }

    public override long Next()
    {
        int index = Random.Shared.Next(0, Values.Count);
        return Values[index];
    }

    public override bool IsBelong(long value)
    {
        return Values.Contains(value);
    }
}