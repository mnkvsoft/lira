namespace Lira.Domain.Handling;

public interface IMiddleware;

public interface IHandler : IMiddleware
{
    internal Task Handle(HttpContextData httpContextData);
}

public interface IAction : IMiddleware
{
    Task Execute(RuleExecutingContext context);
}