using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class GuidParser
{
    private readonly ILogger _logger;

    public GuidParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dataOptionsDto)
    {
        var intervals = IntParser.GetIntervals(dataOptionsDto.Ranges, new Interval<long>(long.MinValue, long.MaxValue), dataOptionsDto.Capacity);
        
        _logger.LogDataRanges(name, intervals);
        
        return new GuidData(name,
            intervals.ToDictionary(p => p.Key, p => (GuidDataRange)new GuidSeqDataRange(p.Key, new Int64Sequence(p.Value))));
    }
}