using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsLoggingEnabled(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("LoggingEnabled");
    }
    public static string GetRulesPath(this IConfiguration configuration)
    {
        var rulesPath = configuration.GetValue<string>("RulesPath");

        if (string.IsNullOrWhiteSpace(rulesPath))
            throw new InvalidOperationException("RulesPath is empty");
        
        return rulesPath;
    }
}
