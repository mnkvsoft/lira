using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace SimpleMockServer.Domain.Configuration;

interface IConfigurationPathProvider
{
    string Path { get; }
    event EventHandler Changed;
}

class ConfigurationPathProvider : IConfigurationPathProvider, IDisposable
{
    public string Path { get; }
    private readonly ILogger _logger;
    
    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;
    public event EventHandler? Changed;
    
    public ConfigurationPathProvider(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        Path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);
        
        _logger.LogInformation($"Rules path for watching: {Path}");

        _fileProvider = new PhysicalFileProvider(Path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;
        WatchForFileChanges();
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
        _logger.LogInformation($"Change was detected in {Path}");
        Changed?.Invoke(this, EventArgs.Empty);
    }
    
    public void Dispose()
    {
        _fileProvider.Dispose();
    }
}
