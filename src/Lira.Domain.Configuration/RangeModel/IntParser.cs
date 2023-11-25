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
        var (intervals, info) = GetIntervalsWithInfo(dto);

        var mode = dto.Mode ?? "seq";
        
        info += Environment.NewLine + "Mode: " + mode; 
        
        _logger.LogInformation(new StringBuilder().AddInfoForLog(name, info, intervals).ToString());

        var infoForData = new StringBuilder().AddInfo(info, intervals).ToString();
        if (mode == "seq")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSeqDataRange(p.Key, new Int64Sequence(p.Value)));
            return new IntData(name, seqDatas, infoForData);
        }

        if (mode == "random")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSetIntervalDataRange(p.Key, p.Value));
            return new IntData(name, seqDatas, infoForData);
        }

        throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
    }

    internal record IntervalsWithCapacity(IReadOnlyDictionary<DataName, Interval<long>> Intervals, string RangeInfo);
    
    private static IntervalsWithCapacity GetIntervalsWithInfo(DataOptionsDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Interval) && !string.IsNullOrEmpty(dto.Start))
            throw new Exception("Only one of the values 'interval', 'start' can be filled");

        if (!string.IsNullOrEmpty(dto.Start))
            return GetIntervalsByCustomCapacity(dto);

        var interval = string.IsNullOrEmpty(dto.Interval)
            ? new Interval<long>(1, long.MaxValue)
            : Interval<long>.Parse(dto.Interval, new PrettyNumberParser<long>());

        return GetIntervalsByAutoCapacity(dto.Ranges, interval);
    }

    private static IntervalsWithCapacity GetIntervalsByCustomCapacity(DataOptionsDto dto)
    {
        if (!PrettyNumberParser<long>.TryParse(dto.Start, out long startInterval))
            throw new Exception($"Field 'start' has not int value '{dto.Start}'");

        long capacity = dto.GetCapacity();
        return new IntervalsWithCapacity(GetIntervalsByCustomCapacity(dto.Ranges, startInterval, capacity), "Capacity: " + capacity);
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

    public static IntervalsWithCapacity GetIntervalsByAutoCapacity(
        string[] ranges,
        Interval<long> interval)
    {
        ulong intervalLength = (ulong)(interval.To - interval.From);

        ulong tempCapacity = intervalLength / (ulong)ranges.Length;
        long capacity = tempCapacity > long.MaxValue ? long.MaxValue : (long)tempCapacity;
        
        var intervals = new Dictionary<DataName, Interval<long>>();

        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            long from = i * capacity + interval.From;
            long to = from + capacity - 1;
            var name = new DataName(range);

            intervals.Add(name, new Interval<long>(from, i == ranges.Length - 1 ? interval.To : to));
        }

        return new IntervalsWithCapacity(intervals, 
            "Interval: " + interval + Environment.NewLine +
            "Capacity(auto): " + capacity);
    }
}