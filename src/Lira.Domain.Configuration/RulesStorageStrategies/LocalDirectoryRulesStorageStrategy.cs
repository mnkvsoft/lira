using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lira.Domain.Configuration.RulesStorageStrategies;

class LocalDirectoryRulesStorageStrategy : IRulesStorageStrategy, IDisposable
{
    private readonly ILogger<LocalDirectoryRulesStorageStrategy> _logger;
    public string Path { get; }
    public event Func<Task>? OnChanged;
    private IChangeToken? _fileChangeToken;
    private readonly PhysicalFileProvider _fileProvider;
    private bool _wasInit;

    public LocalDirectoryRulesStorageStrategy(IConfiguration configuration, ILogger<LocalDirectoryRulesStorageStrategy> logger)
    {
        _logger = logger;
        var path = GetRulesPath(configuration);
        Path = path;

        _fileProvider = new PhysicalFileProvider(path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;
    }

    static string GetRulesPath(IConfiguration configuration)
    {
        var rulesPath = configuration.GetValue<string>("RulesPath");

        if (string.IsNullOrWhiteSpace(rulesPath))
            throw new InvalidOperationException("RulesPath is empty");

        return rulesPath;
    }

    public void InitIfNeed()
    {
        if(_wasInit)
            return;

        _logger.LogInformation($"Rules path for watching: {Path}");
        WatchForFileChanges();
        _wasInit = true;
    }

    private void WatchForFileChanges()
    {
        _fileChangeToken = _fileProvider.Watch("**/*.*");
        _fileChangeToken.RegisterChangeCallback(state => _ = OnChange(), default);
    }

    private async Task OnChange()
    {
        _logger.LogInformation($"Change was detected in {Path}");
        WatchForFileChanges();

        if(OnChanged != null)
            await OnChanged.Invoke();
    }

    public void Dispose() => _fileProvider.Dispose();
}