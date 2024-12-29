namespace Lira.Common;

public static class Paths
{
    private static readonly string Temp = Path.Combine(Path.GetTempPath(), "lira");

    public static string GetTempSubPath(string subPath) => Path.Combine(Path.GetTempPath(), "lira", subPath);
}