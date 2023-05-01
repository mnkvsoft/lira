namespace SimpleMockServer.Domain.DataModel;

public abstract class DataRange<T> : DataRange where T : struct
{
    protected DataRange(DataName name) : base(name)
    {
    }

    public override object NextValue()
    {
        return Next();
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
