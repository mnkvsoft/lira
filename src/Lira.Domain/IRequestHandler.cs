using System.Diagnostics;
using System.Text;
using Lira.Common.Extensions;
using Lira.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain;

public interface IRequestHandler
{
    Task Handle(RequestData requestData, HttpResponse response);
}

class RequestHandler : IRequestHandler
{
    private readonly IReadOnlyCollection<Rule> _rules;
    private readonly bool _isLoggingEnabled;
    private readonly ILogger _logger;

    public RequestHandler(IReadOnlyCollection<Rule> rules, ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _rules = rules;
        _logger = loggerFactory.CreateLogger(GetType());
        _isLoggingEnabled = configuration.IsLoggingEnabled();
    }

    public async Task Handle(RequestData requestData, HttpResponse response)
    {
        var swTotal = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();

        var ctx = new RuleExecutingContext(requestData);
        var matches = await GetMatchedRules(ctx);

        var searchMs = sw.GetElapsedDoubleMilliseconds();
        sw.Restart();

        if (matches.Count == 1)
        {
            var match = matches.First();

            try
            {
                await match.Rule.Handle(new HttpContextData(ctx, new ResponseData(response)));
            }
            catch (Exception exc)
            {
                throw new Exception("An unexpected exception occurred while rule processing. Rule name: " + match.Rule.Info, exc);
            }

            if (_isLoggingEnabled)
            {
                _logger.LogInformation($"Was usage rule (time: {swTotal.GetElapsedDoubleMilliseconds()} ms (search: {searchMs} ms, exe: {sw.GetElapsedDoubleMilliseconds()} ms). protocol: {requestData.Protocol}. weight: {match.Weight}): " + match.Rule.Info);
            }

            return;
        }

        if (matches.Count == 0)
        {
            response.StatusCode = 404;
            await response.WriteAsync("Rule not found");
            return;
        }

        if (matches.Count > 1)
        {
            response.StatusCode = 404;
            await response.WriteAsync(GetErrorMessageForManyRules(matches.Select(x => x.Rule)));
            return;
        }

        throw new Exception("Unknown case");
    }

    private async Task<IReadOnlyCollection<RuleMatchResult.Matched>> GetMatchedRules(RuleExecutingContext ctx)
    {
        var matchedRules = new List<RuleMatchResult.Matched>();

        foreach (var rule in _rules)
        {
            try
            {
                 var matchResult = await rule.IsMatch(ctx);

                 if (matchResult is RuleMatchResult.Matched matched)
                     matchedRules.Add(matched);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occured while rule '{rule.Info}' matching", e);
            }
        }

        if (matchedRules.Count == 0)
            return matchedRules;

        var maxPriorityRule = matchedRules.MaxBy(x => x.Weight);

        return matchedRules
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
            sb.AppendLine("- " + rule.Info);
        }

        return sb.ToString();
    }
}