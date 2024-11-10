using Lira.Common.State;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Lira.Configuration;
using Lira.Domain.Configuration.Rules;

namespace Lira.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    void BeginLoading();
}

class ConfigurationLoader : IAsyncDisposable, IRulesProvider, IConfigurationLoader
{
    private readonly string _path;
    private readonly ILogger<ConfigurationLoader> _logger;
    private readonly ConfigurationReader _configurationReader;
    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;

    private Task<ConfigurationState>? _configurationLoadingTask;

    private ConfigurationState.Ok? _lastOkConfiguration;

    private readonly IStateRepository _stateRepository;

    public ConfigurationLoader(
        IConfiguration configuration,
        IStateRepository stateRepository,
        ILogger<ConfigurationLoader> logger, ConfigurationReader configurationReader)
    {
        _stateRepository = stateRepository;
        _logger = logger;
        _configurationReader = configurationReader;
        _path = configuration.GetRulesPath();

        _fileProvider = new PhysicalFileProvider(_path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;
    }

    public void BeginLoading()
    {
        _configurationLoadingTask = LoadConfiguration(_path);
        _logger.LogInformation($"Rules path for watching: {_path}");
    }

    private async Task OnChange()
    {
        _logger.LogInformation($"Change was detected in {_path}");
        var loadConfigurationTask = LoadConfiguration(_path);
        await loadConfigurationTask;
        _configurationLoadingTask = loadConfigurationTask;
    }

    private async Task<ConfigurationState> LoadConfiguration(string path)
    {
        try
        {
            var (rules, newStates) = await _configurationReader.Read(path);

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
            return new ConfigurationState.Error(DateTime.Now, e);
        }
        finally
        {
            WatchForFileChanges();
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

    private void WatchForFileChanges()
    {
        _fileChangeToken = _fileProvider.Watch("**/*.*");
        _fileChangeToken.RegisterChangeCallback(state => _ = OnChange(), default);
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
            _fileProvider.Dispose();
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