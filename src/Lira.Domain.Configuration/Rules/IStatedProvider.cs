namespace Lira.Domain.Configuration.Rules;

public interface IStatedProvider
{
    Task<ConfigurationState> GetState();
}
