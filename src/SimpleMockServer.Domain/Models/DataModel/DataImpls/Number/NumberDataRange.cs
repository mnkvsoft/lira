namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Number;

abstract class NumberDataRange : DataRange<long>
{
    protected NumberDataRange(DataName name) : base(name)
    {
    }

    public override bool TryParse(string str, out long value) => long.TryParse(str, out value);
}
