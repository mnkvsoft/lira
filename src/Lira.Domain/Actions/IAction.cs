namespace Lira.Domain.Actions;

public interface IAction
{
    Task Execute(RuleExecutingContext context);
}
