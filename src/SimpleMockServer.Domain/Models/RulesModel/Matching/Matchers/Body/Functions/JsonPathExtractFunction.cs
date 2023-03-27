using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body.Functions;

public class JsonPathExtractFunction : IExtractFunction
{
    private readonly string _jpath;
    private readonly ILogger _logger;

    public JsonPathExtractFunction(ILoggerFactory loggerFactory, string jpath)
    {
        _jpath = jpath;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public string? Extract(string? value)
    {
        if (value == null)
            return null;

        JObject o;
        try
        {
            o = JObject.Parse(value);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("json", value);
            return null;
        }

        JToken? token = o.SelectToken(_jpath);
        string? result = token?.Value<string>();
        return result;
    }
}