using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Hex;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class HexParser
{
    private readonly ILogger _logger;

    public HexParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var interval = new Interval<long>(long.MinValue, long.MaxValue);
        ulong capacity = IntParser.GetCapacity(dto.Ranges.Length, dto.Capacity, interval);
        var intervals = IntParser.GetIntervals(dto.Ranges, interval, capacity);
        int bytesCount = dto.BytesCount ?? 32;

        var additionInfo = "Bytes count: " + bytesCount;
        _logger.LogInformation(new StringBuilder().AddInfoForLog(name, capacity, intervals, additionInfo).ToString());

        return new HexData(
            name,
            intervals.ToDictionary(p => p.Key, p =>
            {
                return new HexDataRange(p.Key, p.Value, bytesCount);
            }),
            new StringBuilder().AddInfo(capacity, intervals, additionInfo).ToString());
    }
}