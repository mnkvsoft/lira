using Lira.Common;

namespace Lira.Domain.DataModel.DataImpls.Guid;

public class GuidDataRange : DataRange<System.Guid>
{
    private readonly Int64Sequence _seq;

    public GuidDataRange(DataName name, Int64Sequence seq, string? format) : base(name, format)
    {
        _seq = seq;
    }

    public override bool IsBelong(System.Guid value) => _seq.Interval.InRange(value.ToInt64());

    public override System.Guid Next() => _seq.Next().ToRandomGuid();

    public override bool TryParse(string str, out System.Guid value) => System.Guid.TryParse(str, out value);
}
