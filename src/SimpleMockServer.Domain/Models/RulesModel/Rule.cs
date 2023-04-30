using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel;

public class Rule
{
    public string Name { get; }
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
        ConditionMatcherSet? conditionMatcherSet,
        IReadOnlyCollection<Delayed<IExternalCaller>> callers)
    {
        _responseWriter = responseWriter;
        _requestMatcherSet = matchers;
        _logger = loggerFactory.CreateLogger(GetType());
        Name = name;
        _conditionMatcherSet = conditionMatcherSet;
        _callers = callers;
    }

    public async Task<bool> IsMatch(RequestData request, Guid requestId)
    {
        using var scope = _logger.BeginScope($"Rule: {Name}");

        bool isMatch = await _requestMatcherSet.IsMatch(request);

        if (!isMatch)
            return false;

        if (_conditionMatcherSet == null)
            return true;

        isMatch = await _conditionMatcherSet.IsMatch(request, requestId);

        return isMatch;
    }

    public async Task Execute(HttpContextData httpContextData)
    {
        if (_responseWriter.Delay != null)
            await Task.Delay(_responseWriter.Delay.Value);

        await _responseWriter.Value.Write(httpContextData);

        await httpContextData.Request.SaveBody();
        _ = CallExtrernalSystems(httpContextData.Request);
    }

    private async Task CallExtrernalSystems(RequestData request)
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
            _logger.LogError(ex, $"An error accured while calling '{caller.GetType().Name}'");
        }
    }
}
