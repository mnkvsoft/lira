using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsLoggingEnabled(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("LoggingEnabled");
    }
}
