using ArgValidation;

namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid;

public class GuidData : Data
{
    private readonly DataName _name;

    private readonly IReadOnlyDictionary<DataName, GuidDataRange> _ranges;
    private readonly GuidDataRange _defaultRange;


    public GuidData(DataName name, IReadOnlyDictionary<DataName, GuidDataRange> ranges) : base(name)
    {
        Arg.NotEmpty(ranges, nameof(ranges));
        Arg.NotDefault(name, nameof(name));

        _ranges = ranges;
        _name = name;
        _defaultRange = ranges.First().Value;
    }

    public override DataRange Get(DataName rangeName)
    {
        if (!_ranges.TryGetValue(rangeName, out var range))
            throw new Exception($"For data '{_name}' not found range '{rangeName}'");
        return range;
    }

    public override DataRange GetDefault()
    {
        return _defaultRange;
    }
}
