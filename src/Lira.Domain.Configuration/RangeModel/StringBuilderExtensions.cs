using System.Text;
using Lira.Common;
using Lira.Domain.DataModel;

namespace Lira.Domain.Configuration.RangeModel;

static class StringBuilderExtensions
{
    public static StringBuilder AddInfo<T>(this StringBuilder sb,
        StringBuilder into,
        IReadOnlyDictionary<DataName, Interval<T>> intervals)
        where T : struct, IComparable<T>
    {
        sb.AppendLine(into.ToString());

        sb.AppendLine("Ranges:");

        var nameLength = intervals.Select(x => x.Key.ToString().Length).Max() + 1;
        
        foreach (var pair in intervals)
        {
            sb.AppendLine(pair.Key.ToString().PadRight(nameLength, ' ') + " " + pair.Value);
        }

        return sb;
    }
    public static StringBuilder AddInfoForLog<T>(this StringBuilder sb,
        DataName name,
        StringBuilder info,
        IReadOnlyDictionary<DataName, Interval<T>> intervals)
        where T : struct, IComparable<T>
    {
        sb.AppendLine("Name: " + name);
        sb.AddInfo(info, intervals);
        
        return sb;
    }
}