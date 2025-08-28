namespace Lira.Common;

public static class Paths
{
    public static readonly string GetTempPath = Path.Combine(Path.GetTempPath(), "lira");
    public static string GetTempSubPath(string subPath) => Path.Combine(GetTempPath, subPath);
}