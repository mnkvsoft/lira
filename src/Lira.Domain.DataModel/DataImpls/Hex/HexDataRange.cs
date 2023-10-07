using System.Diagnostics.CodeAnalysis;
using ArgValidation;
using Lira.Common;

namespace Lira.Domain.DataModel.DataImpls.Hex;

public class HexDataRange : DataRange<string>
{
    private readonly Interval<long> _interval;
    private readonly int _bytesCount;
    private readonly int _stringLength;

    public HexDataRange(DataName name, Interval<long> interval, int bytesCount) : base(name, format: null)
    {
        Arg.Validate(bytesCount, nameof(bytesCount))
            .Min(8);

        _bytesCount = bytesCount;
        _interval = interval;
        _stringLength = bytesCount * 2;
    }

    public override string Next()
    {
        var value = Random.Shared.NextInt64(_interval.From, _interval.To);
        var valueBytes = BitConverter.GetBytes(value);
        if (valueBytes.Length == _bytesCount)
            return HexConverter.ToHexString(valueBytes);

        var result = new byte[_bytesCount];
        Array.Copy(valueBytes, result, 8);

        var remainBytes = result.AsSpan(8);
        Random.Shared.NextBytes(remainBytes);

        return HexConverter.ToHexString(result);
    }

    public override bool TryParse(string str, [MaybeNullWhen(false)] out string value)
    {
        value = null;
        if(str.Length != _stringLength)
            return false;

        if(!HexConverter.IsValidHexString(str))
            return false;

        value = str;
        return true;
    }

    public override bool IsBelong(string value)
    {
        var bytes = HexConverter.ToBytes(value);
        var val = BitConverter.ToInt64(bytes.AsSpan(0, 8));
        return _interval.InRange(val);
    }
}
