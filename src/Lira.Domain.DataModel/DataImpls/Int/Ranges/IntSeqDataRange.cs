using Lira.Common.State;

namespace Lira.Domain.DataModel.DataImpls.Int.Ranges;

public class IntSeqDataRange : IntDataRange
{
    private readonly SequenceStateful _seq;

    public IntSeqDataRange(DataName name, SequenceStateful seq) : base(name)
    {
        _seq = seq;
    }

    public override long Next()
    {
        return _seq.Next();
    }

    public override bool IsBelong(long value)
    {
        return _seq.Interval.InRange(value);
    }

    public override IStateful GetStateful() => _seq;
}
