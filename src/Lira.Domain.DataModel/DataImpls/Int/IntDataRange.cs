using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Int;

public abstract class IntDataRange : DataRange<long>
{
    protected IntDataRange(DataName name, string? description) : base(name, format: null, description: description)
    {
    }

    public override bool TryParse(string str, out long value) => long.TryParse(str, out value);

    public abstract IStateful? GetStateful();
}
