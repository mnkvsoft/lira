namespace Lira.Common;

public static class Paths
{
    public static string GetTempSubPath(string subPath) => Path.Combine(Path.GetTempPath(), "lira", subPath);
}