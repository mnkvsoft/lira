using System.Text;
using SimpleMockServer.Domain;
using SimpleMockServer.Domain.Configuration.Rules;

namespace SimpleMockServer.Middlewares;

class RoutingMiddleware : IMiddleware
{
    private readonly IRulesProvider _rulesProvider;
    private readonly IReadOnlyCollection<IStatedProvider> _statedProviders;
    private readonly ILogger _logger;

    public RoutingMiddleware(ILoggerFactory loggerFactory, IRulesProvider rulesProvider, IEnumerable<IStatedProvider> statedProviders)
    {
        _rulesProvider = rulesProvider;
        _statedProviders = statedProviders.ToArray();
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await HandleRequest(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occured");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(e.ToString());
        }
    }

    private async Task HandleRequest(HttpContext context)
    {
        var req = context.Request;
        req.EnableBuffering();

        foreach (var provider in _statedProviders)
        {
            var state = await provider.GetState();
            if (state is ProviderState.Error error)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(error.Exception.ToString());
                return;
            }
        }

        var request = new RequestData(req.Method, req.Path, req.QueryString, req.Headers, req.Query, req.Body);

        var matchesRules = await GetMatchedRules(request);

        if (matchesRules.Count == 1)
        {
            var rule = matchesRules.First();
            await rule.Execute(new HttpContextData(request, context.Response));
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

    private async Task<IReadOnlyCollection<Rule>> GetMatchedRules(RequestData request)
    {
        var allRules = await _rulesProvider.GetRules();

        var requestId = Guid.NewGuid();

        var matchedRules = new List<Rule>();

        foreach (var rule in allRules)
        {
            if (await rule.IsMatch(request, requestId))
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
