using System.Text;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules;

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

        var rulesWithExtInfo = await _rulesProvider.GetRules();

        RuleWithExtInfo[] matchesRules = await GetMatchedRules(request, rulesWithExtInfo);

        if (matchesRules.Length == 1)
        {
            var rule = matchesRules[0];
            _logger.LogInformation($"Matched rule: {rule}");
            await rule.Rule.Execute(new Domain.Models.RulesModel.HttpContextData(context.Request, context.Response));
            return;
        }

        if (matchesRules.Length == 0)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Rule not found");
            return;
        }

        if (matchesRules.Length > 1)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(GetErrorMessageForManyRules(matchesRules));
            return;
        }

        throw new Exception("Unknown case");
    }

    private static async Task<RuleWithExtInfo[]> GetMatchedRules(HttpRequest request, IReadOnlyCollection<RuleWithExtInfo> rulesWithExtInfo)
    {
        Guid requestId = Guid.NewGuid();
        var rules = rulesWithExtInfo
            .Select(r => new
            {
                Task = r.Rule.IsMatch(request, requestId),
                Rule = r
            })
            .ToArray();

        await Task.WhenAll(rules.Select(x => x.Task));

        var matchesRules = rules
            .Where(x => x.Task.Result)
            .Select(x => x.Rule)
            .ToArray();

        return matchesRules;
    }

    private static string GetErrorMessageForManyRules(RuleWithExtInfo[] matchesRules)
    {
        var sb = new StringBuilder("Find more than one rule:");
        sb.AppendLine();

        foreach (var rule in matchesRules)
        {
            sb.AppendLine($"- file {rule.FileName}. " + rule.Rule.Name);
        }

        return sb.ToString();
    }
}
