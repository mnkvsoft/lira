namespace Lira.Domain;

public interface IExternalCaller
{
    Task Call(RequestData request);
}
