using Lira.Domain.DataModel;

namespace Lira.Domain.Configuration.RangeModel;

public static class Utils
{
    public static string GetStateId(DataName rangeName, DataName subRangeName) => $"{rangeName}.{subRangeName}";
}