namespace Lira.Domain;

public interface IHandler
{
    Task Handle(HttpContextData httpContextData);
}