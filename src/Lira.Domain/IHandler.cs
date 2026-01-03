namespace Lira.Domain;

public interface IHandler
{
    internal Task Handle(HttpContextData httpContextData);
}