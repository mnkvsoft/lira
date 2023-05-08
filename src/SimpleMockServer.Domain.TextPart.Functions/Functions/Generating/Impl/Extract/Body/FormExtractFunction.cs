using System.Web;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Matching.Request.Matchers.Body;
using SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body.Extensions;

namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract.Body;

public class FormExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumenFunction
{
    public static string Name => "read.req.body.form";

    private string? _formParamName;
    private readonly ILogger _logger;

    public FormExtractFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public string? Extract(string? value)
    {
        if (value == null)
            return null;

        System.Collections.Specialized.NameValueCollection pars;
        try
        {
            pars = HttpUtility.ParseQueryString(value);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("form", value);
            return null;
        }

        var result = pars[_formParamName];
        return result;
    }

    public object? Get(RequestData request)
    {
        return Extract(request.ReadBody());
    }

    public void SetArgument(string argument)
    {
        _formParamName = argument;
    }
}
