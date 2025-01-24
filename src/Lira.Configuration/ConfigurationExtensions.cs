using Microsoft.Extensions.Configuration;

namespace Lira.Configuration;

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

    public static string? GetLibsPath(this IConfiguration configuration) => configuration.GetValue<string>("LibsPath");
    public static string? GetNugetLibsPath(this IConfiguration configuration) => configuration.GetValue<string>("NugetLibsPath");
}
