using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating;

internal static class ILoggerExtensions
{
    public static void WarnAboutDecreasePerformance(this ILogger logger, string formatName, string body) =>
        logger.LogWarning(
            $"An error occured while parsing to {formatName} '{body}'. " +
            $"It can lead to decrease performance. " +
            $"Use addition rules for url, headers matching.");
}
