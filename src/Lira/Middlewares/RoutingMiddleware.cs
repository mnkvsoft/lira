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
        var executors = await GetRuleExecutors(request);
        var searchMs = sw.GetElapsedDoubleMilliseconds();
        sw.Restart();

        if (executors.Count == 1)
        {
            var executor = executors.First();

            try
            {
                await executor.Execute(context.Response);
            }
            catch (Exception exc)
            {
                throw new Exception("An unexpected exception occurred while rule processing. Rule name: " + executor.Rule.Name, exc);
            }

            if (_isLoggingEnabled)
                _logger.LogInformation($"Was usage rule (times. search: {searchMs} ms, exe: {sw.GetElapsedDoubleMilliseconds()} ms. protocol: {context.Request.Protocol}): " + executor.Rule.Name);
            return;
        }

        if (executors.Count == 0)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Rule not found");
            return;
        }

        if (executors.Count > 1)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(GetErrorMessageForManyRules(executors.Select(x => x.Rule)));
            return;
        }

        throw new Exception("Unknown case");
    }

    private async Task<IReadOnlyCollection<IRuleExecutor>> GetRuleExecutors(RequestData request)
    {
        var allRules = await _rulesProvider.GetRules();

        var requestId = Guid.NewGuid();

        var executors = new List<IRuleExecutor>();

        foreach (var rule in allRules)
        {
            IRuleExecutor? executor;
            try
            {
                executor = await rule.GetExecutor(request, requestId);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occured while rule '{rule.Name}' matching", e);
            }

            if (executor != null)
                executors.Add(executor);
        }

        if (executors.Count == 0)
            return executors;

        if (!_allowMultipleRules)
            return executors;

        var maxPriorityRule = executors.MaxBy(x => x.Weight);

        return executors
            .Where(x => x.Weight.CompareTo(maxPriorityRule!.Weight) == 0)
            .Select(x => x)
            .ToArray();
    }

    private static string GetErrorMessageForManyRules(IEnumerable<Rule> matchedRules)
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