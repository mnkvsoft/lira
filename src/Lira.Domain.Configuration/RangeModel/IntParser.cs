using System.Text;
using Lira.Common;
using Lira.Domain.Configuration.PrettyParsers;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Int;
using Lira.Domain.DataModel.DataImpls.Int.Ranges;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RangeModel;

class IntParser
{
    private readonly ILogger _logger;

    public IntParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var (intervals, capacity) = GetIntervalsWithCapacity(dto);

        var mode = dto.Mode ?? "seq";
        var additionInfo = "Mode: " + mode;
        
        _logger.LogInformation(new StringBuilder().AddInfoForLog(name, capacity, intervals, additionInfo).ToString());

        var info = new StringBuilder().AddInfo(capacity, intervals, additionInfo).ToString();
        if (mode == "seq")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSeqDataRange(p.Key, new Int64Sequence(p.Value)));
            return new IntData(name, seqDatas, info);
        }

        if (mode == "random")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSetIntervalDataRange(p.Key, p.Value));
            return new IntData(name, seqDatas, info);
        }

        throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
    }

    private static (IReadOnlyDictionary<DataName, Interval<long>> Intervals, long Capacity) GetIntervalsWithCapacity(DataOptionsDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Interval) && !string.IsNullOrEmpty(dto.Start))
            throw new Exception("Only one of the values 'interval', 'start' can be filled");

        IReadOnlyDictionary<DataName, Interval<long>> intervals;
        long capacity;

        if (!string.IsNullOrEmpty(dto.Start))
        {
            intervals = GetIntervalsByCustomCapacity(dto, out capacity);
        }
        else
        {
            var interval = string.IsNullOrEmpty(dto.Interval)
                ? new Interval<long>(1, long.MaxValue)
                : Interval<long>.Parse(dto.Interval, new PrettyNumberParser<long>());

            intervals = GetIntervalsByAutoCapacity(dto.Ranges, interval, out capacity);
        }

        return (intervals, capacity);
    }

    private static IReadOnlyDictionary<DataName, Interval<long>> GetIntervalsByCustomCapacity(DataOptionsDto dto, out long capacity)
    {
        if (!PrettyNumberParser<long>.TryParse(dto.Start, out long startInterval))
            throw new Exception($"Field 'start' has not int value '{dto.Start}'");

        if (string.IsNullOrWhiteSpace(dto.Capacity))
            throw new Exception("Field 'capacity' is required if filled 'start' field");

        if (!PrettyNumberParser<long>.TryParse(dto.Capacity, out capacity))
            throw new Exception($"Field 'capacity' has not int value '{dto.Capacity}'");
        
        return GetIntervalsByCustomCapacity(dto.Ranges, startInterval, capacity);
    }

    public static IReadOnlyDictionary<DataName, Interval<long>> GetIntervalsByCustomCapacity(string[] ranges, long startInterval, long capacity)
    {
        var intervals = new Dictionary<DataName, Interval<long>>();
        foreach (string rangeName in ranges)
        {
            long endInterval = startInterval + capacity - 1;
            intervals.Add(new DataName(rangeName), new Interval<long>(startInterval, endInterval));
            startInterval = endInterval + 1;
        }

        return intervals;
    }

    public static IReadOnlyDictionary<DataName, Interval<long>> GetIntervalsByAutoCapacity(
        string[] ranges,
        Interval<long> interval,
        out long capacity)
    {
        ulong intervalLength = (ulong)(interval.To - interval.From);

        ulong tempCapacity = intervalLength / (ulong)ranges.Length;
        capacity = tempCapacity > long.MaxValue ? long.MaxValue : (long)tempCapacity;
        
        var result = new Dictionary<DataName, Interval<long>>();

        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            long from = i * capacity + interval.From;
            long to = from + capacity - 1;
            var name = new DataName(range);

            result.Add(name, new Interval<long>(from, i == ranges.Length - 1 ? interval.To : to));
        }

        return result;
    }
}