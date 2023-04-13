using System.Text;
using SimpleMockServer.ConfigurationProviding.Rules;
using SimpleMockServer.Domain.Models.RulesModel;

namespace SimpleMockServer;

class RoutingMiddleware : IMiddleware
{
    private readonly IRulesProvider _rulesProvider;
    private readonly ILogger _logger;

    public RoutingMiddleware(IRulesProvider rulesProvider, ILoggerFactory loggerFactory)
    {
        _rulesProvider = rulesProvider;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        request.EnableBuffering();

        _logger.LogInformation($"Begin handle {request.Method} {request.Path}");

        var matchesRules = await GetMatchedRules(request);

        if (matchesRules.Count == 1)
        {
            var rule = matchesRules.First();
            _logger.LogInformation($"Matched rule: {rule.Name}");
            await rule.Execute(new HttpContextData(context.Request, context.Response));
            return;
        }

        if (matchesRules.Count == 0)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Rule not found");
            return;
        }

        if (matchesRules.Count > 1)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(GetErrorMessageForManyRules(matchesRules));
            return;
        }

        throw new Exception("Unknown case");
    }

    private async Task<IReadOnlyCollection<Rule>> GetMatchedRules(HttpRequest request)
    {
        var allRules = await _rulesProvider.GetRules();
        
        var requestId = Guid.NewGuid();

        var matchedRules = new List<Rule>();

        foreach (var rule in allRules)
        {
            if(await rule.IsMatch(request, requestId))
                matchedRules.Add(rule);
        }
        
        return matchedRules;
    }

    private static string GetErrorMessageForManyRules(IReadOnlyCollection<Rule> matchedRules)
    {
        var sb = new StringBuilder("Find more than one rule:");
        sb.AppendLine();

        foreach (var rule in matchedRules)
        {
            sb.AppendLine("- " + rule.Name);
        }

        return sb.ToString();
    }
}
