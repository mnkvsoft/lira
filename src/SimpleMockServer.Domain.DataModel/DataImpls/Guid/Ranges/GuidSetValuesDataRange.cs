using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;

public class GuidSetValuesDataRange : GuidDataRange
{
    public IReadOnlyList<long> Values { get; }

    public GuidSetValuesDataRange(DataName name, IReadOnlyList<long> values) : base(name)
    {
        Values = values;
    }

    public override System.Guid Next()
    {
        var index = Random.Shared.Next(0, Values.Count);
        return Values[index].ToGuid();
    }

    public override bool IsBelong(System.Guid value)
    {
        return Values.Contains(value.ToLong());
    }
}
