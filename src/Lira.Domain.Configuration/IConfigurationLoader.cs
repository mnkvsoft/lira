using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Lira.Configuration;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.TextPart;

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

    private readonly StateRepository _stateRepository;

    public ConfigurationLoader(
        IConfiguration configuration,
        StateRepository stateRepository,
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
                var lastStates = _lastOkConfiguration.States;
                await SaveStates(lastStates);
                RestoreStates(newStates, lastStates.ToDictionary(x => x.StateId, x => x.GetState()));
            }
            else
            {
                var savedStates = await _stateRepository.GetStates();
                RestoreStates(newStates, savedStates);
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

    private async Task SaveStates(IReadOnlyCollection<IState> currentStates)
    {
        foreach (var currentState in currentStates)
        {
            currentState.Seal();
        }

        var states = currentStates.Select(s => new KeyValuePair<string, string>(s.StateId, s.GetState()))
            .ToDictionary();
        await _stateRepository.UpdateStates(states);
    }

    public async Task<ConfigurationState> GetState()
    {
        if (_configurationLoadingTask == null)
            throw new Exception("Method BeginLoading() must be called first");

        return await _configurationLoadingTask;
    }

    private void RestoreStates(IReadOnlyCollection<IState> states, IReadOnlyDictionary<string, string> savedStates)
    {
        foreach (var withState in states)
        {
            var stateId = withState.StateId;

            if (savedStates.TryGetValue(stateId, out var state))
                withState.RestoreState(state);
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
                await SaveStates(_lastOkConfiguration.States);
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