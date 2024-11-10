using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel.DataImpls.Guid;

public class GuidDataRange : DataRange<System.Guid>
{
    private readonly SequenceStateful _seq;

    public GuidDataRange(DataName name, SequenceStateful seq, string? format) : base(name, format)
    {
        _seq = seq;
    }

    public override bool IsBelong(System.Guid value) => _seq.Interval.InRange(value.ToInt64());

    public override System.Guid Next() => _seq.Next().ToRandomGuid();

    public override bool TryParse(string str, out System.Guid value) => System.Guid.TryParse(str, out value);

    public IStateful GetState() => _seq;
}