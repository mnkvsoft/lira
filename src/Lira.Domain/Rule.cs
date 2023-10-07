using Microsoft.Extensions.Logging;

namespace Lira.Domain;

public record PathNameMap(int Index, string? Name);

public class Rule
{
    public string Name { get; }
    public IReadOnlyCollection<PathNameMap> PathNameMaps { get; }
    
    private readonly ILogger _logger;
    private readonly RequestMatcherSet _requestMatcherSet;
    private readonly ConditionMatcherSet? _conditionMatcherSet;
    private readonly Delayed<ResponseWriter> _responseWriter;
    private readonly IReadOnlyCollection<Delayed<IExternalCaller>> _callers;

    public Rule(
        string name,
        ILoggerFactory loggerFactory,
         Delayed<ResponseWriter> responseWriter,
        RequestMatcherSet matchers,
        IReadOnlyCollection<PathNameMap> pathNameMaps,
        ConditionMatcherSet? conditionMatcherSet, 
        IReadOnlyCollection<Delayed<IExternalCaller>> callers)
    {
        _responseWriter = responseWriter;
        _requestMatcherSet = matchers;
        _logger = loggerFactory.CreateLogger(GetType());
        Name = name;
        _conditionMatcherSet = conditionMatcherSet;
        _callers = callers;
        PathNameMaps = pathNameMaps;
    }

    public async Task<RuleMatchResult> IsMatch(RequestData request, Guid requestId)
    {
        var matchResult = await _requestMatcherSet.IsMatch(request);

        if (matchResult is RuleMatchResult.NotMatched notMatched)
            return notMatched;

        var matched = (RuleMatchResult.Matched)matchResult;
        if (_conditionMatcherSet == null)
            return matched;

        bool conditionsIsMatch = await _conditionMatcherSet.IsMatch(request, requestId);

        if (conditionsIsMatch)
            return matched;

        return RuleMatchResult.NotMatchedInstance;
    }

    public async Task Execute(HttpContextData httpContextData)
    {
        if (_responseWriter.Delay != null)
            await Task.Delay(_responseWriter.Delay.Value);

        await _responseWriter.Value.Write(httpContextData);

        await httpContextData.Request.SaveBody();
        _ = CallExternalSystems(httpContextData.Request);
    }

    private async Task CallExternalSystems(RequestData request)
    {
        await Task.WhenAll(_callers.Select(caller => TryCall(caller, request)));
    }

    private async Task TryCall(Delayed<IExternalCaller> caller, RequestData request)
    {
        if (caller.Delay != null)
            await Task.Delay(caller.Delay.Value);

        try
        {
            await caller.Value.Call(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while calling '{caller.GetType().Name}'");
        }
    }
}
