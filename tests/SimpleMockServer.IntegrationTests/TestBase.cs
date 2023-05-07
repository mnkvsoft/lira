using System.Reflection;

namespace SimpleMockServer.IntegrationTests;

[Parallelizable(ParallelScope.All)]
public class TestBase
{
    protected static string GetCurrentDirectory() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    protected static string CreateRulesPath()
    {
        string rulesPath = Path.Combine(GetCurrentDirectory(), "rules", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rulesPath);
        return rulesPath;
    }
}
