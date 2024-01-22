using ArgValidation;

namespace Lira.Domain.DataModel;

public abstract class Data<T> : Data where T : notnull
{
    private readonly DataName _name;

    private readonly IReadOnlyDictionary<DataName, DataRange<T>> _ranges;

    protected Data(DataName name, IReadOnlyDictionary<DataName, DataRange<T>> ranges, string info) : base(name, info)
    {
        Arg.NotEmpty(ranges, nameof(ranges));
        Arg.NotDefault(name, nameof(name));

        _ranges = ranges;
        _name = name;
    }

    public override DataRange Get(DataName rangeName)
    {
        if (!_ranges.TryGetValue(rangeName, out var range))
            throw new Exception($"For interval '{_name}' not found range '{rangeName}'");
        return range;
    }

    public override DataRange? Find(DataName name)
    {
        _ranges.TryGetValue(name, out var range);
        return range;
    }
}
