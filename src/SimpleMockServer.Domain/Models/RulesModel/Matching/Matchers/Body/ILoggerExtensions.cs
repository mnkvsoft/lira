using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

internal static class ILoggerExtensions
{
    public static void WarnAboutDecreasePerformance(this ILogger logger, string formatName, string body) =>
        logger.LogWarning(
            $"An error occured while parsing to {formatName} '{body}'. " +
            $"It can lead to decrease performance. " +
            $"Use addition rules for url, headers matching.");
}