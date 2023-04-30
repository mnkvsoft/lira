using System.Web;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body;

public class FormExtractFunction : IBodyExtractFunction, IGeneratingPrettyFunction, IWithStringArgumenFunction
{
    public static string Name => "extract.body.form";

    private string _formParamName;
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

    public object? Generate(RequestData request)
    {
        return Extract(request.ReadBody());
    }

    public void SetArgument(string argument)
    {
        _formParamName = argument;
    }
}
