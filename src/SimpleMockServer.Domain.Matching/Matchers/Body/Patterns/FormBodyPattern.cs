using System.Web;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Matching;

namespace SimpleMockServer.Domain.Matching.Matchers.Body.Patterns;

class FormBodyPattern : IBodyPattern
{
    private IReadOnlyDictionary<string, IValuePattern> _formParamToPattern;
    private readonly ILogger _logger;

    public FormBodyPattern(ILoggerFactory loggerFactory, IReadOnlyDictionary<string, IValuePattern> formParamToPattern)
    {
        _formParamToPattern = formParamToPattern;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public bool IsMatch(string body)
    {
        System.Collections.Specialized.NameValueCollection pars;
        try
        {
            pars = HttpUtility.ParseQueryString(body);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("form", body);
            return false;
        }

        foreach (var pair in _formParamToPattern)
        {
            var name = pair.Key;
            var pattern = pair.Value;

            var current = pars[name] ?? "";

            if (!pattern.IsMatch(current))
                return false;
        }

        return true;
    }
}