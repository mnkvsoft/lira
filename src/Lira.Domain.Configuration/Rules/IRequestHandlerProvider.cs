namespace Lira.Domain.Configuration.Rules;

public interface IRequestHandlerProvider
{
    Task<IRequestHandler> GetRequestHandler();
}
