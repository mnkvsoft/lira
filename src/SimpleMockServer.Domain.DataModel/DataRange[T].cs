namespace SimpleMockServer.Domain.DataModel;

public abstract class DataRange<T> : DataRange where T : notnull
{
    private readonly string? _format;

    protected DataRange(DataName name, string? format) : base(name)
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

    public override bool IsBelong(string strValue)
    {
        if (!TryParse(strValue, out var value))
            return false;

        return IsBelong(value);
    }

    public abstract T Next();

    public abstract bool TryParse(string str, out T value);

    public abstract bool IsBelong(T value);
}
