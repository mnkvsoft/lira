using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class Rule
{
    public string Name { get; }
    private readonly ILogger _logger;
    private readonly RequestMatcherSet _requestMatcherSet;
    private readonly ConditionMatcherSet? _conditionMatcherSet;
    private readonly ResponseWriter _responseWriter;

    public Rule(
        string name,
        ILoggerFactory loggerFactory,
        ResponseWriter responseWriter,
        RequestMatcherSet matchers,
        ConditionMatcherSet? conditionMatcherSet)
    {
        _responseWriter = responseWriter;
        _requestMatcherSet = matchers;
        _logger = loggerFactory.CreateLogger(GetType());
        Name = name;
        _conditionMatcherSet = conditionMatcherSet;
    }

    public async Task Execute(HttpContextData httpContextData)
    {
        await _responseWriter.Write(httpContextData);
    }

    public async Task<bool> IsMatch(HttpRequest request, Guid requestId)
    {
        using var scope = _logger.BeginScope($"Rule: {Name}");

        bool isMatch = await _requestMatcherSet.IsMatch(request);

        if(!isMatch)
            return false;

        if (_conditionMatcherSet == null)
            return true;

        isMatch = await _conditionMatcherSet.IsMatch(request, requestId);

        return isMatch;
    }
}
