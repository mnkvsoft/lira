using System;
using System.Text;
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

        ulong capacity = GetCapacity(dto.Ranges.Length, dto.Capacity, interval);
        var intervals = GetIntervals(dto.Ranges, interval, capacity);

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

    public static IReadOnlyDictionary<DataName, Interval<long>> GetIntervals(
        string[] ranges,
        Interval<long> interval,
        ulong capacity)
    {
        var result = new Dictionary<DataName, Interval<long>>();

        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            long from = (long)((ulong)i * capacity) + interval.From;
            long to = (long)((ulong)from + capacity - 1);
            var name = new DataName(range);

            result.Add(name, new Interval<long>(from, to));
        }

        return result;
    }

    public static ulong GetCapacity(int rangesCount, string? capacityStr, Interval<long> interval)
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

        var restForLastRange = capacity * ((ulong)rangesCount - 1); 
        if(intervalLength - restForLastRange <= 0)
        {
            throw new Exception($"Capacity value {capacityStr} is invalid for current count of ranges ({rangesCount}) " +
                                $"and current interval value ({intervalLength})");
        }

        return capacity;
    }
}