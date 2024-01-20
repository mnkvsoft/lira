using System.Text;
using Lira.Common;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Hex;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RangeModel;

class HexParser
{
    private readonly ILogger _logger;

    public HexParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }
    
    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var fullInfo = new StringBuilder().AppendLine("Type: hex");
        
        // for this value it is possible to define 1 million ranges, which seems sufficient
        long capacity = 1000_000_000_000; 
        
        // we use a custom interval so that when adding new ranges, the old ones remain relevant
        var intervals = IntParser.GetIntervalsByCustomCapacity(dto.Ranges, long.MinValue, capacity);
        int bytesCount = dto.BytesCount ?? 32;

        string info = 
            "Capacity(hardcoded): " + capacity + Constants.NewLine +
            "Bytes count: " + bytesCount;
        fullInfo.AppendLine(info);
        
        _logger.LogDebug(new StringBuilder().AddInfoForLog(name, fullInfo, intervals).ToString());

        return new HexData(
            name,
            intervals.ToDictionary(p => p.Key, p => new HexDataRange(p.Key, p.Value, bytesCount)),
            new StringBuilder().AddInfo(fullInfo, intervals).ToString());
    }
}