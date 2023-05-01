namespace SimpleMockServer.Domain.DataModel.DataImpls.Number;

public abstract class NumberDataRange : DataRange<long>
{
    protected NumberDataRange(DataName name) : base(name)
    {
    }

    public override bool TryParse(string str, out long value) => long.TryParse(str, out value);
}
