using Lira.Domain.Handling.Generating.History;

namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;

record FaultResponseGenerationHandler(WriteStatDependencies HistoryDependencies) : IHandler
{
    public Task Handle(HttpContextData httpContextData)
    {
        httpContextData.Response.Abort();
        if (HistoryDependencies.WriteHistoryMode is WriteHistoryMode.Write writeHistoryMode)
        {
            HistoryDependencies.Storage.Add(
                writeHistoryMode.RuleName,
                DateTime.UtcNow,
                httpContextData.RuleExecutingContext.RequestData,
                RequestHandleResult.Fault.Instance);
        }
        return Task.CompletedTask;
    }
}