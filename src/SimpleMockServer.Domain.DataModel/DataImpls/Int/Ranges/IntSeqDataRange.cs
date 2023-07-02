namespace SimpleMockServer.Domain.DataModel.DataImpls.Int.Ranges;

public class IntSeqDataRange : IntDataRange
{
    public Int64Sequence Sequence { get; }

    public IntSeqDataRange(DataName name, Int64Sequence seq) : base(name)
    {
        Sequence = seq;
    }

    public override long Next()
    {
        return Sequence.Next();
    }

    public override bool IsBelong(long value)
    {
        return Sequence.Interval.InRange(value);
    }
}
