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

        var info = new StringBuilder().AddInfo(name, capacity, intervals, "Bytes count: " + bytesCount).ToString();
        _logger.LogInformation(info);

        return new HexData(
            name,
            intervals.ToDictionary(p => p.Key, p =>
            {
                return new HexDataRange(p.Key, p.Value, bytesCount);
            }),
            info);
    }
}