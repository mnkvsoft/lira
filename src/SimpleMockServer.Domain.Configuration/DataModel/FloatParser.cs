﻿using System.Globalization;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.Configuration.PrettyParsers;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Float;
using SimpleMockServer.Domain.DataModel.DataImpls.Float.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class FloatParser
{
    private readonly ILogger _logger;

    public FloatParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public Data Parse(DataName name, DataOptionsDto dto)
    {
        decimal unit = dto.Unit ?? 0.01m;
        Interval<decimal> interval;

        if (string.IsNullOrWhiteSpace(dto.Interval))
        {
            interval = new Interval<decimal>(unit, 1_000_000);
        }
        else
        {
            interval = Interval<decimal>.Parse(dto.Interval, new PrettyNumberParser<decimal>());
            
            if (!IsDividedWithoutRemainder(interval.From, unit))
                throw new Exception($"The lower bound of the interval must be a multiple of '{unit}'");
            
            if (!IsDividedWithoutRemainder(interval.To, unit))
                throw new Exception($"The upper bound of the interval must be a multiple of '{unit}'");
        }

        var intervals = GetIntervals(dto.Ranges, interval, dto.Capacity, unit);

        _logger.LogDataRanges(name, intervals);
        
        return new FloatData(name,
            intervals.ToDictionary(p => p.Key, p => (DataRange<decimal>)new FloatSetIntervalDataRange(p.Key, p.Value, GetDecimals(unit))));
    }

    private static IReadOnlyDictionary<DataName, Interval<decimal>> GetIntervals(
        string[] ranges,
        Interval<decimal> interval,
        string? capacityStr,
        decimal unit)
    {
        decimal capacity = GetCapacity(ranges.Length, capacityStr, interval, unit);
        var result = new Dictionary<DataName, Interval<decimal>>();

        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            decimal from = i * capacity + interval.From;
            decimal to = (i == ranges.Length - 1) ? interval.To : from + capacity - unit;
            var name = new DataName(range);

            result.Add(name, new Interval<decimal>(from, to));
        }

        return result;
    }

    private static decimal GetCapacity(int rangesCount, string? capacityStr, Interval<decimal> interval, decimal unit)
    {
        decimal intervalLength = interval.To - interval.From;

        if (string.IsNullOrWhiteSpace(capacityStr) || capacityStr == "auto")
            return Math.Round(intervalLength / rangesCount, GetDecimals(unit));

        if (!PrettyNumberParser<ulong>.TryParse(capacityStr, out ulong capacity))
            throw new ArgumentException("Invalid capacity value: " + capacityStr);

        if (intervalLength < capacity)
            throw new Exception($"Capacity value {capacityStr} more than interval length {intervalLength}");

        if (!IsDividedWithoutRemainder(capacity, unit))
            throw new Exception($"Capacity must be a multiple of '{unit}'");
        
        return capacity;
    }

    private static bool IsDividedWithoutRemainder(decimal value, decimal unit) => value % unit == 0;

    private static int GetDecimals(decimal value) => value.ToString(CultureInfo.InvariantCulture).TrimEnd('0').Split('.')[1].Length;
}