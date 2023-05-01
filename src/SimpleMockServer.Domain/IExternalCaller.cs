namespace SimpleMockServer.Domain;

public interface IExternalCaller
{
    Task Call(RequestData request);
}
