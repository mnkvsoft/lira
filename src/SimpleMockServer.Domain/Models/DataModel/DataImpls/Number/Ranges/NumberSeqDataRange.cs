namespace SimpleMockServer.Domain.Models.DataModel.DataImpls.Number.Ranges;

class NumberSeqDataRange : NumberDataRange
{
    public Int64Sequence Sequence { get; }

    public NumberSeqDataRange(DataName name, Int64Sequence seq) : base(name)
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
