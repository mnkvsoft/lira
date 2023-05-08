using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleMockServer.Domain.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body.Extensions;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class JsonPathExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumenFunction
{
    public static string Name => "read.req.body.jpath";

    private string _jpath = "";
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

    public object? Get(RequestData request)
    {
        return Extract(request.ReadBody());
    }

    public void SetArgument(string argument)
    {
        _jpath = argument;
    }
}
