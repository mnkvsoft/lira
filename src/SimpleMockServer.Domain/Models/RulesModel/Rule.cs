using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class Rule
{
    public string Name { get; }
    private readonly ILogger _logger;
    private readonly RequestMatcherSet _matchers;
    private readonly ResponseWriter _responseWriter;

    public Rule(string name, ILoggerFactory loggerFactory, ResponseWriter responseWriter, RequestMatcherSet matchers)
    {
        _responseWriter = responseWriter;
        _matchers = matchers;
        _logger = loggerFactory.CreateLogger(GetType());
        Name = name;
    }

    public async Task Execute(HttpContextData httpContextData)
    {
        await _responseWriter.Write(httpContextData);
    }

    public async Task<bool> IsMatch(HttpRequest request)
    {
        using var scope = _logger.BeginScope($"Rule: {Name}");

        foreach(var matcher in _matchers)
        {
            bool isMatch = await matcher.IsMatch(request);
            if(!isMatch)
            {
                _logger.LogInformation($"Matcher {matcher.GetType().Name} result: {isMatch}");
                return false;
            }
        }
        return true;
    }
}