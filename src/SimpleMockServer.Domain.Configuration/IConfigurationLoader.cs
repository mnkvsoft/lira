using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SimpleMockServer.Domain.Configuration.DataModel;
using SimpleMockServer.Domain.Configuration.Rules;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Configuration.Templating;
using SimpleMockServer.Domain.Configuration.Variables;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    Task Load();
}

class ConfigurationLoader : IDisposable, IRulesProvider, IDataProvider, IConfigurationLoader
{
    private readonly string _path;
    private readonly ILogger _logger;
    
    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;
    
    private readonly GlobalVariablesParser _globalVariablesParser;
    private readonly RulesLoader _rulesLoader;
    private readonly DataLoader _dataLoader;
    
    private Task<LoadResult> _loadTask;
    private ConfigurationState? _providerState;
    
    public ConfigurationLoader(ILoggerFactory loggerFactory, IConfiguration configuration, GlobalVariablesParser globalVariablesParser, RulesLoader rulesLoader, DataLoader dataLoader)
    {
        _globalVariablesParser = globalVariablesParser;
        _rulesLoader = rulesLoader;
        _dataLoader = dataLoader;
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);

        _loadTask = Load(_path);
        
        _logger.LogInformation($"Rules path for watching: {_path}");

        _fileProvider = new PhysicalFileProvider(_path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;
        
        WatchForFileChanges();
    }

    record LoadResult(IReadOnlyCollection<Rule> Rules, Dictionary<DataName, Data> Datas); 
    public Task Load() => Load(_path);
    private async Task<LoadResult> Load(string path)
    {
        var datas = await _dataLoader.Load(path);
        
        var context = new ParsingContext(new VariableSet(), new TemplateSet(), RootPath: path, CurrentPath: path);
        
        var variables = await _globalVariablesParser.Load(context, path);
        var rules = await _rulesLoader.LoadRules(path, context with { Variables = variables});

        return new LoadResult(rules, datas);
    }

    async Task<IReadOnlyCollection<Rule>> IRulesProvider.GetRules()
    {
        var (rules, _) = await _loadTask;
        return rules;
    }

    Data IDataProvider.GetData(DataName name)
    {
        var (_, datas) = _loadTask.Result;
        return datas[name];
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
            _providerState = new ConfigurationState.Ok();
        }
        catch (Exception e)
        {
            _providerState = new ConfigurationState.Error(e);
        }

        return _providerState;
    }

    public void Dispose()
    {
        _fileProvider.Dispose();
    }
}
