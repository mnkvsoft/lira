namespace Lira.Domain.Handling;

public interface IAction
{
    Task Execute(RuleExecutingContext context);
}