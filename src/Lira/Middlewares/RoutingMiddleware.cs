using System.Diagnostics;
using System.Text;
using Lira.Domain;
using Lira.Common;
using Lira.Domain.Configuration;
using Lira.Domain.Configuration.Rules;
using HttpContextData = Lira.Domain.HttpContextData;
using IRuleMatchWeight = Lira.Domain.IRuleMatchWeight;
using RequestData = Lira.Domain.RequestData;
using Lira.Configuration;

namespace Lira.Middlewares;

class RoutingMiddleware : IMiddleware
{
    private readonly IRulesProvider _rulesProvider;
    private readonly IConfigurationLoader _configurationLoader;
    private readonly ILogger _logger;
    private readonly bool _allowMultipleRules;
    private readonly bool _isLoggingEnabled;

    public RoutingMiddleware(ILoggerFactory loggerFactory, IRulesProvider rulesProvider, IConfigurationLoader configurationLoader,
        IConfiguration configuration)
    {
        _rulesProvider = rulesProvider;
        _configurationLoader = configurationLoader;
        _logger = loggerFactory.CreateLogger(GetType());
        _allowMultipleRules = configuration.GetValue<bool>("AllowMultipleRules");
        _isLoggingEnabled = configuration.IsLoggingEnabled();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/sys"))
        {
            await next(context);
            return;
        }

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

        var state = await _configurationLoader.GetState();
        if (state is ConfigurationState.Error error)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(error.Exception.ToString());
            return;
        }

        var request = new RequestData(req.Method, req.Path, req.QueryString, req.Headers, req.Query, req.Body);

        Stopwatch sw = Stopwatch.StartNew();
        var matchesRules = await GetMatchedRules(request);
        var searchMs = sw.GetElapsedDoubleMilliseconds();
        sw.Restart();
        
        if (matchesRules.Count == 1)
        {
            var rule = matchesRules.First();
            request.PathNameMaps = rule.PathNameMaps;
            await rule.Execute(new HttpContextData(request, context.Response));

            if(_isLoggingEnabled)
                _logger.LogInformation($"Was usage rule (times. search: {searchMs} ms, exe: {sw.GetElapsedDoubleMilliseconds()} ms. protocol: {context.Request.Protocol}): " + rule.Name);
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

        if (!_allowMultipleRules)
        {
            var result = new List<Rule>();

            foreach (var rule in allRules)
            {
                var matchResult = await rule.IsMatch(request, requestId);

                if (matchResult is RuleMatchResult.Matched)
                    result.Add(rule);
            }

            return result;
        }

        var matchedRules = new List<(Rule Rule, IRuleMatchWeight Weight)>();

        foreach (var rule in allRules)
        {
            RuleMatchResult matchResult;
            try
            {
                matchResult = await rule.IsMatch(request, requestId);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occured while rule '{rule.Name}' matching", e);
            }
             
            if (matchResult is RuleMatchResult.Matched matched)
                matchedRules.Add((rule, matched.Weight));
        }

        if (matchedRules.Count == 0)
            return Array.Empty<Rule>();

        var maxPriorityRule = matchedRules.MaxBy(x => x.Weight);

        return matchedRules
            .Where(x => maxPriorityRule.Weight.CompareTo(x.Weight) == 0)
            .Select(x => x.Rule)
            .ToArray();
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