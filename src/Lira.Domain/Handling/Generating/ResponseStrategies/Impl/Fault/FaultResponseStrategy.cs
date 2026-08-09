namespace Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;

public record FaultResponseStrategy : IResponseStrategy
{
    public static readonly FaultResponseStrategy Instance = new();

    Task IResponseStrategy.Handle(RuleExecutingContext ruleExecutingContext, Domain.IResponseWriter responseWriter)
    {
        responseWriter.Abort();
        return Task.CompletedTask;
    }
}