using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleMockServer.Domain.Matching;

namespace SimpleMockServer.Domain.Matching.Matchers.Body.Patterns;

class JsonPathBodyPattern : IBodyPattern
{
    private IReadOnlyDictionary<string, IValuePattern> _jsonPathToPattern;
    private readonly ILogger _logger;

    public JsonPathBodyPattern(ILoggerFactory loggerFactory, IReadOnlyDictionary<string, IValuePattern> jsonPathToPattern)
    {
        _jsonPathToPattern = jsonPathToPattern;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public bool IsMatch(string body)
    {
        JObject o;
        try
        {
            o = JObject.Parse(body);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("json", body);
            return false;
        }

        foreach (var pair in _jsonPathToPattern)
        {
            var path = pair.Key;
            var pattern = pair.Value;

            JToken? token = o.SelectToken(path);

            string? value = token?.Value<string>();

            if (value == null)
                return false;

            if (!pattern.IsMatch(value))
                return false;
        }

        return true;
    }
}