using Lira.Common;
using Lira.Domain.Handling.Generating;
using Lira.Domain.Handling.Generating.ResponseStrategies;

namespace Lira.Domain.Handling;

public interface IResponseGenerator
{
    internal Task Generate(HttpContextData httpContextData);
}

class ResponseGenerator(WriteStatDependencies writeStatDependencies, Factory<IResponseStrategy> responseStrategyFactory) : IResponseGenerator
{
    public async Task Generate(HttpContextData httpContextData)
    {
        var responseStrategy = responseStrategyFactory();

        await WriteHistoryIfNeed(
            httpContextData,
            responseWriter => responseStrategy.Handle(httpContextData.RuleExecutingContext, responseWriter));
    }

    private async Task WriteHistoryIfNeed(HttpContextData httpContextData, Func<IResponseWriter, Task> action)
    {
        var writeHistoryMode = writeStatDependencies.WriteHistoryMode as WriteHistoryMode.Write;

        var savingResponseWriter = writeHistoryMode != null
            ? new SavingResponseWriter(httpContextData.ResponseWriter)
            : null;

        var writer = savingResponseWriter ?? httpContextData.ResponseWriter;

        try
        {
            await action(writer);

            if (writeHistoryMode != null)
            {
                writeStatDependencies.Storage.Add(
                    writeHistoryMode.RuleName,
                    DateTime.UtcNow,
                    httpContextData.RuleExecutingContext.RequestData,
                    savingResponseWriter?.GetRequestHandleResult() ?? throw new Exception("Something went wrong"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
