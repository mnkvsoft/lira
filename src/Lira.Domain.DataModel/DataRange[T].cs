using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.DataModel;

public abstract class DataRange<T> : DataRange where T : notnull
{
    private readonly string? _format;

    protected DataRange(DataName name, string? format, string? description) : base(name, description)
    {
        _format = format;
    }

    public override object NextValue()
    {
        T next = Next();

        if (_format != null && next is IFormattable formattable)
            return formattable.ToString(_format, formatProvider: null);

        return next;
    }

    public override bool ValueIsBelong(string strValue)
    {
        if (!TryParse(strValue, out var value))
            return false;

        return IsBelong(value);
    }

    public abstract T Next();

    public abstract bool TryParse(string str, [MaybeNullWhen(false)] out T value);

    public abstract bool IsBelong(T value);
}
