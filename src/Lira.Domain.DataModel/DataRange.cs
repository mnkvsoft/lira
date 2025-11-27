namespace Lira.Domain.DataModel;

public abstract class DataRange
{
    public DataName Name { get; }
    public string? Description { get; }
    public abstract dynamic NextValue();

    public abstract bool ValueIsBelong(string value);

    protected DataRange(DataName name, string? description)
    {
        Name = name;
        Description = description;
    }
}
