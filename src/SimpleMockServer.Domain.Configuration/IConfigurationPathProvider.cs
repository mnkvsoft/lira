using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Domain.Configuration.Rules;

interface IConfigurationPathProvider
{
    string Path { get; }
    event EventHandler Changed;
}

class ConfigurationPathProvider : IConfigurationPathProvider, IDisposable
{
    public string Path { get; }
    private readonly FileSystemWatcher _watcher;
    public event EventHandler? Changed;
    
    public ConfigurationPathProvider(IConfiguration configuration)
    {
        Path = configuration.GetValue<string>(ConfigurationName.ConfigurationPath);
        
        _watcher = new FileSystemWatcher(Path);
        _watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;

        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
    }
    
    private void OnChanged(object sender, FileSystemEventArgs e) => 
        Changed?.Invoke(this, EventArgs.Empty);

    private void OnCreated(object sender, FileSystemEventArgs e) => 
        Changed?.Invoke(this, EventArgs.Empty);

    private void OnDeleted(object sender, FileSystemEventArgs e) =>
        Changed?.Invoke(this, EventArgs.Empty);

    private void OnRenamed(object sender, RenamedEventArgs e) =>
        Changed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _watcher.Dispose();
    }
}
