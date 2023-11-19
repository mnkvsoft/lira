using System.Text;
using Microsoft.Extensions.Logging;
using Lira.Common;
using Lira.Domain.Configuration.DataModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Guid;

namespace Lira.Domain.Configuration.DataModel;

class GuidParser
{
    private readonly ILogger _logger;

    public GuidParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var interval = new Interval<long>(long.MinValue, long.MaxValue);
        ulong capacity = IntParser.GetCapacity(dto.Ranges.Length, dto.Capacity, interval);
        var intervals = IntParser.GetIntervals(dto.Ranges, interval, capacity);

        _logger.LogInformation(new StringBuilder().AddInfoForLog(name, capacity, intervals).ToString());

        return new GuidData(
            name,
            intervals.ToDictionary(p => p.Key, p => new GuidDataRange(p.Key, new Int64Sequence(p.Value), dto.Format)),
            new StringBuilder().AddInfo( capacity, intervals).ToString());
    }
}