using ArgValidation;

namespace SimpleMockServer.Domain.Models.DataModel;

public abstract class Data
{
    public DataName Name { get; }
    public Data(DataName name)
    {
        Arg.NotDefault(name, nameof(name));
        Name = name;
    }

    public abstract DataRange GetDefault();
    public abstract DataRange Get(DataName rangeName);
}