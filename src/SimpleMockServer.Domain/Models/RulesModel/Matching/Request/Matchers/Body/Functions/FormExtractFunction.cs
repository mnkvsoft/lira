using System.Web;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body.Functions;

public class FormExtractFunction : IExtractFunction
{
    private readonly string _formParamName;
    private readonly ILogger _logger;

    public FormExtractFunction(ILoggerFactory loggerFactory, string formParamName)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _formParamName = formParamName;
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
}