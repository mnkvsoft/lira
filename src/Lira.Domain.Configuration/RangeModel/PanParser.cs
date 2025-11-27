using System.Text;
using Lira.Common;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;
using Lira.Domain.DataModel.DataImpls.Pan;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RangeModel;

class PanParser
{
    private readonly ILogger _logger;

    public PanParser(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public Data Parse(DataName name, DataOptionsDto dto)
    {
        var fullInfo = new StringBuilder().AppendLine("Type: pan");

        long capacity = 1000;

        var intervals = IntParser.GetIntervalsByCustomCapacity(dto.Ranges, 0, capacity);

        var bins = dto.Bins?.Length > 0 ? dto.Bins : [4, 5];

        string info =
            "Capacity(hardcoded): " + capacity + Constants.NewLine +
            "Bins: " + string.Join(", ", bins);
        fullInfo.AppendLine(info);

        _logger.LogDebug(new StringBuilder().AddInfoForLog(name, fullInfo, intervals).ToString());

        return new PanData(
            name,
            intervals.ToDictionary(p => p.Key, p => new PanDataRange(p.Key, new Interval<int>((int)p.Value.From, (int)p.Value.To), bins, dto.Description)),
            new StringBuilder().AddInfo(fullInfo, intervals).ToString());
    }
}