using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;

public class GuidSeqDataRange : GuidDataRange
{
    private readonly Int64Sequence _seq;

    public GuidSeqDataRange(DataName name, Int64Sequence seq) : base(name)
    {
        _seq = seq;
    }

    public override bool IsBelong(System.Guid value)
    {
        return _seq.Interval.InRange(value.ToLong());
    }

    public override System.Guid Next()
    {
        return _seq.Next().ToGuid();
    }
}
