namespace Lira.Domain.Handling.Actions;

public interface IAction : IHandler
{
    protected Task Execute(RuleExecutingContext context);

    Task IHandler.Handle(HttpContextData httpContextData) => Execute(httpContextData.RuleExecutingContext);
}
