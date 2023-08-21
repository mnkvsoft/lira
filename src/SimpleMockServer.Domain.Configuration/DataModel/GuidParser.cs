using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;

namespace SimpleMockServer.Domain.Configuration.DataModel;

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

        var info = new StringBuilder().AddInfo(name, capacity, intervals).ToString();
        _logger.LogInformation(info);

        return new GuidData(
            name,
            intervals.ToDictionary(p => p.Key, p => new GuidDataRange(p.Key, new Int64Sequence(p.Value), dto.Format)),
            info);
    }
}