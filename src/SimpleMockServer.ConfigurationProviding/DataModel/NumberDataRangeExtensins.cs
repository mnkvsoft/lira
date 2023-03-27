using SimpleMockServer.Domain.Models.DataModel;
using SimpleMockServer.Domain.Models.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.Models.DataModel.DataImpls.Guid.Ranges;
using SimpleMockServer.Domain.Models.DataModel.DataImpls.Number;
using SimpleMockServer.Domain.Models.DataModel.DataImpls.Number.Ranges;

namespace SimpleMockServer.ConfigurationProviding.DataModel;

static class NumberDataRangeExtensins
{
    public static GuidDataRange ToGuidDataRange(this NumberDataRange numberDataRange)
    {
        switch (numberDataRange)
        {
            case NumberSeqDataRange seq: return new GuidSeqDataRange(seq.Name, seq.Sequence);
            case NumberSetIntervalDataRange interval: return new GuidSetIntervalDataRange(interval.Name, interval.Interval);
            case NumberSetValuesDataRange values: return new GuidSetValuesDataRange(values.Name, values.Values);
            default: throw new ArgumentException("Unknown type: " + numberDataRange.GetType());
        }
    }

    public static IReadOnlyDictionary<DataName, GuidDataRange> ToGuidRangesDictionary(this IReadOnlyDictionary<DataName, NumberDataRange> dictionary)
    {
        return dictionary.ToDictionary(x => x.Key, x => x.Value.ToGuidDataRange());
    }
}