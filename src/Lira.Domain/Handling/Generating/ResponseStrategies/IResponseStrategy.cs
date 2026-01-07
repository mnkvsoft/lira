namespace Lira.Domain.Handling.Generating.ResponseStrategies;

public interface IResponseStrategy
{
    internal Task Handle(
        RuleExecutingContext ruleExecutingContext,
        IResponseWriter responseWriter);
}