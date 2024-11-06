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

namespace Lira.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    void ProvokeLoad();
}

class ConfigurationLoader : IDisposable, IRulesProvider, IConfigurationLoader
{
    private readonly string _path;
    private readonly ILogger _logger;

    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;

    private readonly RangesLoader _rangesLoader;

    private Task<IReadOnlyCollection<Rule>> _loadTask;
    private ConfigurationState? _providerState;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RangesProvider _rangesProvider;

    public ConfigurationLoader(
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        RangesLoader rangesLoader,
        RangesProvider rangesProvider)
    {
        _rangesProvider = rangesProvider;
        _rangesLoader = rangesLoader;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();

        _loadTask = Load(_path);

        _logger.LogInformation($"Rules path for watching: {_path}");

        _fileProvider = new PhysicalFileProvider(_path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;

        WatchForFileChanges();
    }


    public void ProvokeLoad()
    {
        // loading was init in constructor
        // invoke only for create instanse
    }

    private async Task<IReadOnlyCollection<Rule>> Load(string path)
    {
        _logger.LogInformation("Loading rules...");
        var sw = Stopwatch.StartNew();
        try
        {

            var ranges = await _rangesLoader.Load(path);
            _rangesProvider.Ranges = ranges;

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

            var globalVariablesParser = provider.GetRequiredService<DeclaredItemsLoader>();

            var variables = await globalVariablesParser.Load(context, path);

            var rulesLoader = provider.GetRequiredService<RulesLoader>();
            var rules = await rulesLoader.LoadRules(path, context with { DeclaredItems = variables });

            _logger.LogInformation($"{rules.Count} rules were successfully loaded ({(int)sw.ElapsedMilliseconds} ms)");

            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occured while load rules");
            throw;
        }
    }


    async Task<IReadOnlyCollection<Rule>> IRulesProvider.GetRules()
    {
        var rules = await _loadTask;
        return rules;
    }

    private void WatchForFileChanges()
    {
        _fileChangeToken = _fileProvider.Watch("**/*.*");
        _fileChangeToken.RegisterChangeCallback(Notify, default);
    }

    private void Notify(object? state)
    {
        OnChange();
        WatchForFileChanges();
    }

    private void OnChange()
    {
        _logger.LogInformation($"Change was detected in {_path}");
        _providerState = null;
        _loadTask = Load(_path);
    }

    public async Task<ConfigurationState> GetState()
    {
        if (_providerState != null)
            return _providerState;

        try
        {
            await _loadTask;
            _providerState = new ConfigurationState.Ok(DateTime.Now);
        }
        catch (Exception e)
        {
            _providerState = new ConfigurationState.Error(DateTime.Now, e);
        }

        return _providerState;
    }

    public void Dispose()
    {
        _fileProvider.Dispose();
    }
}
