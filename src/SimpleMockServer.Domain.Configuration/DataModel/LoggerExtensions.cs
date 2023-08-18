using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.Configuration.DataModel;

static class LoggerExtensions
{
    public static void LogDataRanges<T>(this ILogger logger,
        DataName name,
        IReadOnlyDictionary<DataName, Interval<T>> intervals,
        string? additionInfo = null)
        where T : struct, IComparable<T>
    {
        var sb = new StringBuilder("Data: " + name).AppendLine();

        if (additionInfo != null)
        {
            sb.AppendLine(additionInfo);
        }

        sb.AppendLine("Ranges:");

        foreach (var pair in intervals)
        {
            sb.AppendLine(pair.Key.ToString().PadRight(10, ' ') + " " + pair.Value);
        }

        logger.LogInformation(sb.ToString());
    }
}