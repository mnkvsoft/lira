using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Lira.Configuration;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Templating;
using Lira.Domain.Configuration.Variables;
using System.Diagnostics;
using Lira.Domain.Configuration.CustomSets;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.System;

namespace Lira.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    void BeginLoading();
}

class ConfigurationLoader : IAsyncDisposable, IRulesProvider, IConfigurationLoader
{
    private readonly string _path;
    private readonly ILogger _logger;

    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;

    private readonly RangesLoader _rangesLoader;

    private Task<ConfigurationState>? _beginLoadingTask;

    private ConfigurationState? _providerState;
    private ConfigurationState.Ok? _lastOkConfiguration;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly StateRepository _stateRepository;
    private Guid Id = Guid.NewGuid();


    public ConfigurationLoader(
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        RangesLoader rangesLoader,
        StateRepository stateRepository)
    {
        _rangesLoader = rangesLoader;
        _stateRepository = stateRepository;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();

        _fileProvider = new PhysicalFileProvider(_path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;
    }

    public void BeginLoading()
    {
        _beginLoadingTask = LoadConfigurationOnStart(_path).ContinueWith(x => _providerState = x.Result);
        _logger.LogInformation($"Rules path for watching: {_path}");
    }

    private async Task<ConfigurationState> LoadConfigurationOnStart(string path)
    {
        try
        {
            var configurationState = await ReadConfiguration(path);

            if (configurationState is ConfigurationState.Ok ok)
            {
                await RestoreStates(ok);
                _lastOkConfiguration = ok;
            }

            return configurationState;
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

    private async Task<ConfigurationState> ReadConfiguration(string path)
    {
        try
        {
            _logger.LogInformation("Loading rules...");
            var sw = Stopwatch.StartNew();

            var templates = await TemplatesLoader.Load(path);
            var customSets = await CustomSetsLoader.Load(path);

            var context = new ParsingContext(
                new DeclaredItems(),
                templates,
                customSets,
                RootPath: path,
                CurrentPath: path);

            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var ranges = await _rangesLoader.Load(path);
            var rulesProvider = provider.GetRequiredService<RangesProvider>();
            rulesProvider.Ranges = ranges;

            var globalVariablesParser = provider.GetRequiredService<DeclaredItemsLoader>();

            var variables = await globalVariablesParser.Load(context, path);

            var rulesLoader = provider.GetRequiredService<RulesLoader>();
            var rules = await rulesLoader.LoadRules(path, context with { DeclaredItems = variables });

            _logger.LogInformation($"{rules.Count} rules were successfully loaded ({(int)sw.ElapsedMilliseconds} ms)");
            var sequence = provider.GetRequiredService<Sequence>();

            return new ConfigurationState.Ok(DateTime.Now, rules, ranges, sequence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occured while load rules");
            return new ConfigurationState.Error(DateTime.Now, ex);
        }
    }

    async Task<IReadOnlyCollection<Rule>> IRulesProvider.GetRules()
    {
        if (_providerState != null)
        {
            return GetFromState(_providerState);
        }

        if(_beginLoadingTask == null)
            throw new Exception("Method BeginLoading() must be called first");

        return GetFromState(await _beginLoadingTask);

        IReadOnlyCollection<Rule> GetFromState(ConfigurationState state)
        {
            if (state is ConfigurationState.Ok ok)
            {
                _logger.LogInformation($"GetRules count: " +  ok.Rules.Count);
                return ok.Rules;
            }

            var error = (ConfigurationState.Error)state;

            throw new Exception("An error occured while loading configuration: " + error.Exception.Message);
        }
    }

    private void WatchForFileChanges()
    {
        _fileChangeToken = _fileProvider.Watch("**/*.*");
        _fileChangeToken.RegisterChangeCallback(Notify, default);
    }

    private void Notify(object? state) => _ = OnChange();

    private async Task OnChange()
    {
        _logger.LogInformation($"Change was detected in {_path}");
        var state = await ReadConfiguration(_path);

        if (state is ConfigurationState.Ok ok)
        {
            if (_lastOkConfiguration != null)
                await SaveStates(_lastOkConfiguration);

            await RestoreStates(ok);

            _lastOkConfiguration = ok;

        }

        _providerState = state;

        WatchForFileChanges();
    }

    private async Task SaveStates(ConfigurationState.Ok okState)
    {
        var currentStates = GetWithStates(okState);
        foreach (var currentState in currentStates)
        {
            currentState.Seal();
        }

        var states = currentStates.Select(s => new KeyValuePair<string, string>(s.StateId, s.GetState())).ToDictionary();
        await _stateRepository.UpdateStates(states);
    }

    public async Task<ConfigurationState> GetState()
    {
        if (_providerState != null)
            return _providerState;

        if(_beginLoadingTask == null)
            throw new Exception("Method BeginLoading() must be called first");

        return await _beginLoadingTask;
    }

    private async Task RestoreStates(ConfigurationState.Ok ok)
    {
        var savesStates = await _stateRepository.GetStates();
        var withStates = GetWithStates(ok);

        foreach (var withState in withStates)
        {
            var stateId = withState.StateId;

            if (savesStates.TryGetValue(stateId, out var state))
                withState.RestoreState(state);
        }
    }

    private static IReadOnlyCollection<IWithState> GetWithStates(ConfigurationState.Ok ok)
    {
        // var withStates = ok.Ranges
        //     .Select(x => x.Value)
        //     .Cast<IWithState>()
        //     .ToList();
        //
        // withStates.Add(ok.Sequence);
        // return withStates;

        return [ok.Sequence];
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
                await SaveStates(_lastOkConfiguration);
                _logger.LogInformation("States have been saved " + Id);
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
