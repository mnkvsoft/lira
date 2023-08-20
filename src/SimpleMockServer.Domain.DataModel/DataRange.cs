namespace SimpleMockServer.Domain.DataModel;

public abstract class DataRange
{
    public DataName Name { get; }
    public abstract object NextValue();

    public abstract bool ValueIsBelong(string value);

    protected DataRange(DataName name)
    {
        Name = name;
    }
}
