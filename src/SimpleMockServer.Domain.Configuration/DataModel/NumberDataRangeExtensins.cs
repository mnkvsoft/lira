using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;
using SimpleMockServer.Domain.DataModel.DataImpls.Int;
using SimpleMockServer.Domain.DataModel.DataImpls.Int.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

static class NumberDataRangeExtensions
{
    public static IReadOnlyDictionary<DataName, GuidDataRange> ToGuidRangesDictionary(this IReadOnlyDictionary<DataName, DataRange<long>> dictionary)
    {
        return dictionary.ToDictionary(x => x.Key, x => x.Value.ToGuidDataRange());
    }
    
    public static IReadOnlyDictionary<DataName, DataRange<long>> ToLongRangesDictionary(this IReadOnlyDictionary<DataName, IntDataRange> dictionary)
    {
        return dictionary.ToDictionary(p => p.Key, p => (DataRange<long>)p.Value);
    }

    private static GuidDataRange ToGuidDataRange(this DataRange<long> intDataRange)
    {
        return intDataRange switch
        {
            IntSeqDataRange seq => new GuidSeqDataRange(seq.Name, seq.Sequence),
            IntSetIntervalDataRange interval => new GuidSetIntervalDataRange(interval.Name, interval.Interval),
            IntSetValuesDataRange values => new GuidSetValuesDataRange(values.Name, values.Values),
            _ => throw new UnsupportedInstanceType(intDataRange)
        };
    }
}
