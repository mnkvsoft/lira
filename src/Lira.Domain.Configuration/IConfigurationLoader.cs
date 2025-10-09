using System.Text;
using Lira.Common.Extensions;
using Lira.Common.State;
using Microsoft.Extensions.Logging;
using Lira.Domain.Configuration.Rules;

namespace Lira.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    void BeginLoading();
}

class ConfigurationLoader(
    IRulesStorageStrategy rulesStorageStrategy,
    IStateRepository stateRepository,
    ILogger<ConfigurationLoader> logger, ConfigurationReader configurationReader) : IAsyncDisposable, IRulesProvider, IConfigurationLoader
{
    private readonly ILogger<ConfigurationLoader> _logger = logger;
    private readonly ConfigurationReader _configurationReader = configurationReader;
    private readonly IRulesStorageStrategy _rulesStorageStrategy = rulesStorageStrategy;
    private readonly IStateRepository _stateRepository = stateRepository;

    private Task<ConfigurationState>? _configurationLoadingTask;
    private ConfigurationState.Ok? _lastOkConfiguration;

    public void BeginLoading()
    {
        _rulesStorageStrategy.OnChanged += OnChange;
        _configurationLoadingTask = LoadConfiguration();
    }

    private async Task OnChange()
    {
        var loadConfigurationTask = LoadConfiguration();
        await loadConfigurationTask;
        _configurationLoadingTask = loadConfigurationTask;
    }

    private async Task<ConfigurationState> LoadConfiguration()
    {
        try
        {
            _rulesStorageStrategy.InitIfNeed();
            var (rules, newStates) = await _configurationReader.Read(_rulesStorageStrategy.Path);

            if (_lastOkConfiguration != null)
            {
                var lastStates = _lastOkConfiguration.Statefuls;
                await SaveStates(lastStates);

                var savedValues = await _stateRepository.GetStates();
                IReadOnlyDictionary<string, IState> states = lastStates.ToDictionary(x => x.StateId, x => x.GetState());
                RestoreStates(newStates, savedValues, states);
            }
            else
            {
                var savedStates = await _stateRepository.GetStates();
                RestoreStates(newStates, savedStates, statesInMemory: null);
            }

            _lastOkConfiguration = new ConfigurationState.Ok(DateTime.Now, rules, newStates);
            return _lastOkConfiguration;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occured while load rules");

            var sb = new StringBuilder();
            sb.AppendLine("An unexpected error occured while loading rules.");
            sb.AppendLine(e.GetMessagesChain());

            _logger.LogInformation(sb.ToString());
            return new ConfigurationState.Error(DateTime.Now, e);
        }
    }

    async Task<IReadOnlyCollection<Rule>> IRulesProvider.GetRules()
    {
        if (_configurationLoadingTask == null)
            throw new Exception("Method BeginLoading() must be called first");

        var state = await _configurationLoadingTask;

        if (state is ConfigurationState.Ok ok)
            return ok.Rules;

        var error = (ConfigurationState.Error)state;

        throw new Exception("An error occured while loading configuration: " + error.Exception.Message);
    }


    private async Task SaveStates(IReadOnlyCollection<IStateful> currentStates)
    {
        var states = currentStates.Select(s => new KeyValuePair<string, string>(s.StateId, s.GetState().Value))
            .ToDictionary();
        await _stateRepository.UpdateStates(states);
    }

    public async Task<ConfigurationState> GetState()
    {
        if (_configurationLoadingTask == null)
            throw new Exception("Method BeginLoading() must be called first");

        return await _configurationLoadingTask;
    }

    private void RestoreStates(
        IReadOnlyCollection<IStateful> statefuls,
        IReadOnlyDictionary<string, string> savedStates,
        IReadOnlyDictionary<string, IState>? statesInMemory)
    {
        foreach (var withState in statefuls)
        {
            var stateId = withState.StateId;

            if (statesInMemory != null)
            {
                if (statesInMemory.TryGetValue(stateId, out var stateInMemory))
                {
                    withState.RestoreState(stateInMemory);
                    continue;
                }
            }

            if (savedStates.TryGetValue(stateId, out var stateValue))
                withState.RestoreState(stateValue);
        }
    }

    bool _disposed;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            _rulesStorageStrategy.OnChanged -= OnChange;
            if (_lastOkConfiguration != null)
            {
                foreach (var stateful in _lastOkConfiguration.Statefuls)
                {
                    stateful.GetState().Seal();
                }

                await SaveStates(_lastOkConfiguration.Statefuls);
            }
            else
            {
                _logger.LogInformation("States have not been saved");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occured while disposing configuration");
        }
        finally
        {
            _disposed = true;
        }
    }
}