namespace Lira.Domain.DataModel;

public abstract class DataRange(DataName name)
{
    public DataName Name => name;
    public abstract dynamic NextValue();

    public abstract bool ValueIsBelong(string value);
}
