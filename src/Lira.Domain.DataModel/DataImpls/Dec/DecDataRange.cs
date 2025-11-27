namespace Lira.Domain.DataModel.DataImpls.Dec;

public abstract class DecDataRange : DataRange<decimal>
{
    protected DecDataRange(DataName name, string? description) : base(name, format: null, description)
    {
    }

    public override bool TryParse(string str, out decimal value) => decimal.TryParse(str, out value);
}
