using System.Text;
using Lira.Common;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Guid;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RangeModel;

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
        var intervals = IntParser.GetIntervalsByAutoCapacity(dto.Ranges, interval, out long capacity);

        _logger.LogInformation(new StringBuilder().AddInfoForLog(name, capacity, intervals).ToString());

        return new GuidData(
            name,
            intervals.ToDictionary(p => p.Key, p => new GuidDataRange(p.Key, new Int64Sequence(p.Value), dto.Format)),
            new StringBuilder().AddInfo( capacity, intervals).ToString());
    }
}