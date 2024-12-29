namespace Lira.Domain.DataModel.DataImpls.Dec;

public abstract class DecDataRange : DataRange<decimal>
{
    protected DecDataRange(DataName name) : base(name, format: null)
    {
    }

    public override bool TryParse(string str, out decimal value) => decimal.TryParse(str, out value);
}
