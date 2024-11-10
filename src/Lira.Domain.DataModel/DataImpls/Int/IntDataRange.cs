using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Int;

public abstract class IntDataRange : DataRange<long>
{
    protected IntDataRange(DataName name) : base(name, format: null)
    {
    }

    public override bool TryParse(string str, out long value) => long.TryParse(str, out value);

    public abstract IStateful? GetStateful();
}
