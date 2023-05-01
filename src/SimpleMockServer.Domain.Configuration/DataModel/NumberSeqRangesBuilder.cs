using SimpleMockServer.Common;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class NumberSeqRangesBuilder
{
    private readonly Dictionary<DataName, long> _nameToStart = new();
    private long _lastStartRange = 0;

    public void Add(DataName rangeName, long startRange)
    {
        if (startRange <= _lastStartRange)
            throw new ArgumentOutOfRangeException($"Start range '{rangeName}' must be more previous range");

        _nameToStart.Add(rangeName, startRange);
    }

    public Dictionary<DataName, Int64Interval> BuildIntervals()
    {
        var result = new Dictionary<DataName, Int64Interval>();

        var keyValues = _nameToStart.ToArray();
        for (var i = 0; i < keyValues.Length; i++)
        {
            var name = keyValues[i].Key;
            var from = keyValues[i].Value;
            var to = i == keyValues.GetMaxIndex() ? long.MaxValue : keyValues[i + 1].Value - 1;

            result.Add(name, new Int64Interval(from, to));
        }

        return result;
    }

    public int Count => _nameToStart.Count;
}
