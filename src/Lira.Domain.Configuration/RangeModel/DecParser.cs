using System.Globalization;
using System.Text;
using Lira.Common;
using Lira.Common.PrettyParsers;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Dec;
using Lira.Domain.DataModel.DataImpls.Dec.Ranges;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RangeModel;

class DecParser
{
    private readonly ILogger _logger;

    public DecParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public Data Parse(DataName name, DataOptionsDto dto)
    {
        decimal unit = dto.Unit ?? 0.01m;
        var fullInfo = new StringBuilder().AppendLine("Type: dec");

        var (intervals, info) = GetIntervalsWithInfo(dto, unit);
        fullInfo.AppendLine(info);
        fullInfo.AppendLine($"Unit:{(dto.Unit == null ? "(default)" : "")} " + unit);

        _logger.LogDebug(new StringBuilder().AddInfoForLog(name, fullInfo, intervals).ToString());

        return new DecData(
            name,
            intervals.ToDictionary(p => p.Key, p => (DataRange<decimal>)new DecSetIntervalDataRange(p.Key, p.Value, GetDecimals(unit))),
            new StringBuilder().AddInfo(fullInfo, intervals).ToString());
    }

    record IntervalsWithCapacity(IReadOnlyDictionary<DataName, Interval<decimal>> Intervals, string RangeInformation);

    private static IntervalsWithCapacity GetIntervalsWithInfo(
        DataOptionsDto dto,
        decimal unit)
    {
        if (!string.IsNullOrEmpty(dto.Interval) && !string.IsNullOrEmpty(dto.Start))
            throw new Exception("Only one of the values 'interval', 'start' can be filled");

        if (!string.IsNullOrEmpty(dto.Start))
            return GetIntervalsByCustomCapacity(dto, unit);

        return GetIntervalsByAutoCapacity(dto, unit);
    }

    private static IntervalsWithCapacity GetIntervalsByAutoCapacity(DataOptionsDto dto, decimal unit)
    {
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

        decimal intervalLength = interval.To - interval.From;
        decimal capacity = Math.Round(intervalLength / dto.Ranges.Length, GetDecimals(unit));

        var result = new Dictionary<DataName, Interval<decimal>>();

        var ranges = dto.Ranges;
        for (int i = 0; i < ranges.Length; i++)
        {
            string range = ranges[i];
            decimal from = i * capacity + interval.From;
            decimal to = i == ranges.Length - 1 ? interval.To : from + capacity - unit;

            var name = new DataName(range);

            result.Add(name, new Interval<decimal>(from, to));
        }

        return new IntervalsWithCapacity(result,
            $"Interval({(dto.Interval == null ? "default" : "manual")}) : " + interval + Constants.NewLine +
            "Capacity(auto): " + capacity);
    }

    private static IntervalsWithCapacity GetIntervalsByCustomCapacity(DataOptionsDto dto, decimal unit)
    {
        if (!PrettyNumberParser<decimal>.TryParse(dto.Start, out decimal startInterval))
            throw new Exception($"Field 'start' has not int value '{dto.Start}'");

        if (!PrettyNumberParser<decimal>.TryParse(dto.Capacity, out decimal rangeLength))
            throw new Exception($"Field 'capacity' has not decimal value '{dto.Start}'");

        var intervals = new Dictionary<DataName, Interval<decimal>>();
        foreach (string rangeName in dto.Ranges)
        {
            decimal endInterval = startInterval + rangeLength - unit;
            intervals.Add(new DataName(rangeName), new Interval<decimal>(startInterval, endInterval));
            startInterval = endInterval + unit;
        }

        return new IntervalsWithCapacity(intervals, "Capacity: " + rangeLength);
    }

    private static bool IsDividedWithoutRemainder(decimal value, decimal unit) => value % unit == 0;

    private static int GetDecimals(decimal value) => value.ToString(CultureInfo.InvariantCulture).TrimEnd('0').Split('.')[1].Length;
}