using ArgValidation;
using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel;

public abstract class Data
{
    public DataName Name { get; }
    public string Info { get; }

    protected Data(DataName name, string info)
    {
        Arg.NotDefault(name, nameof(name));
        Arg.NotNullOrWhitespace(info, nameof(info));

        Name = name;
        Info = info;
    }

    public abstract DataRange Get(DataName rangeName);
    public abstract DataRange? Find(DataName rangeName);
    public abstract IEnumerable<IStateful> GetStates();
}
