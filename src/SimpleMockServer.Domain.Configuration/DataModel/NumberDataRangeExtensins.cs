using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;
using SimpleMockServer.Domain.DataModel.DataImpls.Number;
using SimpleMockServer.Domain.DataModel.DataImpls.Number.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

static class NumberDataRangeExtensions
{
    public static IReadOnlyDictionary<DataName, GuidDataRange> ToGuidRangesDictionary(this IReadOnlyDictionary<DataName, NumberDataRange> dictionary)
    {
        return dictionary.ToDictionary(x => x.Key, x => x.Value.ToGuidDataRange());
    }

    private static GuidDataRange ToGuidDataRange(this NumberDataRange numberDataRange)
    {
        switch (numberDataRange)
        {
            case NumberSeqDataRange seq: return new GuidSeqDataRange(seq.Name, seq.Sequence);
            case NumberSetIntervalDataRange interval: return new GuidSetIntervalDataRange(interval.Name, interval.Interval);
            case NumberSetValuesDataRange values: return new GuidSetValuesDataRange(values.Name, values.Values);
            default: throw new UnsupportedInstanceType(numberDataRange);
        }
    }
}
