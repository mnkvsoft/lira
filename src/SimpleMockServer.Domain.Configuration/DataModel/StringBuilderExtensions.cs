using System.Text;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.Configuration.DataModel;

static class StringBuilderExtensions
{
    public static StringBuilder AddInfo<T, TCapacity>(this StringBuilder sb,
        TCapacity capacity,
        IReadOnlyDictionary<DataName, Interval<T>> intervals,
        string? additionInfo = null)
        where T : struct, IComparable<T>
    {
        sb.AppendLine("Capacity: " + capacity);

        if (additionInfo != null)
        {
            sb.AppendLine(additionInfo);
        }

        sb.AppendLine("Ranges:");
        foreach (var pair in intervals)
        {
            sb.AppendLine(pair.Key.ToString().PadRight(10, ' ') + " " + pair.Value);
        }

        return sb;
    }
    public static StringBuilder AddInfoForLog<T, TCapacity>(this StringBuilder sb,
        DataName name,
        TCapacity capacity,
        IReadOnlyDictionary<DataName, Interval<T>> intervals,
        string? additionInfo = null)
        where T : struct, IComparable<T>
    {
        sb.AppendLine("Name: " + name);
        sb.AddInfo(capacity, intervals, additionInfo);
        
        return sb;
    }
}