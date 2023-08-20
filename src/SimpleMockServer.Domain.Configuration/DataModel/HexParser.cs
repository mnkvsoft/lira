using System.Linq;
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
        var intervals = IntParser.GetIntervals(dto.Ranges, new Interval<long>(long.MinValue, long.MaxValue), dto.Capacity);
        
        _logger.LogDataRanges(name, intervals);
        
        return new HexData(
            name,
            intervals.ToDictionary(p => p.Key, p => new HexDataRange(p.Key, p.Value, dto.BytesCount ?? 32)));
    }
}