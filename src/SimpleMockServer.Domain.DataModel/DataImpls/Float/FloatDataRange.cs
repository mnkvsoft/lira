namespace SimpleMockServer.Domain.DataModel.DataImpls.Float;

public abstract class FloatDataRange : DataRange<decimal>
{
    protected FloatDataRange(DataName name) : base(name, format: null)
    {
    }

    public override bool TryParse(string str, out decimal value) => decimal.TryParse(str, out value);
}
