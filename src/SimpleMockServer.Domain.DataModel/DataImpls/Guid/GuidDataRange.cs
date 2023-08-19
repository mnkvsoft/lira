using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid;

public class GuidDataRange : DataRange
{
    private readonly Int64Sequence _seq;
    private readonly string? _format;

    public GuidDataRange(DataName name, Int64Sequence seq, string? format) : base(name)
    {
        _seq = seq;
        _format = format;
    }

    public override object NextValue()
    {
        System.Guid nextValue = _seq.Next().ToGuid();
        if (_format == null)
            return nextValue;

        return nextValue.ToString(_format);
    }

    public override bool IsBelong(string value)
    {
        if (!System.Guid.TryParse(value, out var guid))
            return false;

        return _seq.Interval.InRange(guid.ToLong());
    }
}
