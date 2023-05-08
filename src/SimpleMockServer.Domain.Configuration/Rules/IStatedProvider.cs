namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IStatedProvider
{
    Task<ProviderState> GetState();
}