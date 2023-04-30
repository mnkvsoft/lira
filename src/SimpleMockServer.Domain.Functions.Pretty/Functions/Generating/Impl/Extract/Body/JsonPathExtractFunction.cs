using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body;

public class JsonPathExtractFunction : IBodyExtractFunction, IGeneratingPrettyFunction, IWithStringArgumenFunction
{
    public static string Name => "extract.body.jpath";

    private string _jpath;
    private readonly ILogger _logger;

    public JsonPathExtractFunction(ILoggerFactory loggerFactory)
    {
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

        var token = o.SelectToken(_jpath);
        var result = token?.Value<string>();
        return result;
    }

    public object? Generate(RequestData request)
    {
        return Extract(request.ReadBody());
    }

    public void SetArgument(string argument)
    {
        _jpath = argument;
    }
}
