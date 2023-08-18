using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.Configuration.PrettyParsers;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Int;
using SimpleMockServer.Domain.DataModel.DataImpls.Int.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class IntParser
{
    private readonly ILogger _logger;

    public IntParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var mode = dto.Mode ?? "seq";
        
        var interval = string.IsNullOrEmpty(dto.Interval)
            ? new Interval<long>(1, long.MaxValue)
            : Interval<long>.Parse(dto.Interval, new PrettyNumberParser<long>());
        
        var intervals = GetIntervals(dto.Ranges, interval, dto.Capacity);

        _logger.LogDataRanges(name, intervals, "Mode: " + mode);
        
        if (mode == "seq")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSeqDataRange(p.Key, new Int64Sequence(p.Value)));
            return new IntData(name, seqDatas);
        }

        if (mode == "random")
        {
            var seqDatas = intervals.ToDictionary(p => p.Key,
                p => (DataRange<long>)new IntSetIntervalDataRange(p.Key, p.Value));
            return new IntData(name, seqDatas);
        }

        throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
    }

    public static IReadOnlyDictionary<DataName, Interval<long>> GetIntervals(
        string[] ranges,
        Interval<long> interval,
        string? capacityStr)
    {
        ulong capacity = GetCapacity(ranges.Length, capacityStr, interval);
        var result = new Dictionary<DataName, Interval<long>>();

        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            long from = (long)((ulong)i * capacity) + interval.From;
            long to = i == ranges.Length - 1 ? interval.To : (long)((ulong)from + capacity - 1);
            var name = new DataName(range);

            result.Add(name, new Interval<long>(from, to));
        }

        return result;
    }

    private static ulong GetCapacity(int rangesCount, string? capacityStr, Interval<long> interval)
    {
        ulong intervalLength = (ulong)(interval.To - interval.From);

        if (string.IsNullOrWhiteSpace(capacityStr) || capacityStr == "auto")
        {
            return intervalLength / (ulong)rangesCount;
        }

        if (!PrettyNumberParser<ulong>.TryParse(capacityStr, out ulong capacity))
            throw new ArgumentException("Invalid capacity value: " + capacityStr);

        if (intervalLength < capacity)
            throw new Exception($"Capacity value {capacityStr} more than interval length {intervalLength}");

        return capacity;
    }
}