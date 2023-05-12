namespace SimpleMockServer.Domain.Configuration;

static class DirectoryHelper
{
    public static IReadOnlyCollection<string> GetFiles(string path, string pattern)
        => Directory.GetFiles(path, pattern, SearchOption.AllDirectories).OrderBy(p => p).ToArray();
}
