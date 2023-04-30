namespace SimpleMockServer.Domain.Models.RulesModel;

public interface IExternalCaller
{
    Task Call(RequestData request);
}
