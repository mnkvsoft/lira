using System.Collections;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;

namespace Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading.Nuget;

class AvailableRepositories : IEnumerable<SourceRepository>
{
    private readonly IReadOnlyCollection<SourceRepository> _repositories;
    private readonly IReadOnlyCollection<SourceRepository> _repositoriesWithGlobalFolder;

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly SourceCacheContext _cache;
    private readonly NuGet.Common.ILogger _nugetLogger;
    private readonly string _globalPackagesFolder;

    public AvailableRepositories(ISettings settings, ILogger logger, NuGet.Common.ILogger nugetLogger)
    {
        _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
        _repositories = GetSourceRepositories(settings);

        // first add local folder
        var repositoriesWithGlobalFolder = new List<SourceRepository>
        {
            new(new PackageSource(_globalPackagesFolder, "global folder"), Repository.Provider.GetVisualStudio())
        };
        repositoriesWithGlobalFolder.AddRange(_repositories);

        _repositoriesWithGlobalFolder = repositoriesWithGlobalFolder;

        _logger = logger;
        _settings = settings;
        _nugetLogger = nugetLogger;
        _cache = new SourceCacheContext();
    }

    private static IReadOnlyCollection<SourceRepository> GetSourceRepositories(ISettings settings)
    {
        var repositories = new List<SourceRepository>();
        foreach (var source in SettingsUtility.GetEnabledSources(settings))
        {
            repositories.Add(source.ProtocolVersion switch
            {
                2 => Repository.Factory.GetCoreV2(source),
                3 => Repository.Factory.GetCoreV3(source),
                _ => throw new Exception($"Unknown protocol version: {source.ProtocolVersion}")
            });
        }

        return repositories;
    }

    public async Task<SourcePackageDependencyInfo?> ResolvePackage(PackageIdentity package, NuGetFramework framework, CancellationToken ct)
    {
        return await TryInvoke(_repositoriesWithGlobalFolder, async r =>
        {
            var dependencyInfoResource = await r.GetResourceAsync<DependencyInfoResource>(ct);
            return await dependencyInfoResource.ResolvePackage(package, framework, _cache, _nugetLogger, ct);
        });
    }

    public async Task<PackageReaderBase> GetPackageReader(PackageIdentity packageIdentity, CancellationToken ct)
    {
        var package = GlobalPackagesFolderUtility.GetPackage(packageIdentity, _globalPackagesFolder);
        if (package != null)
            return package.PackageReader;

        string source = null!;

        var packageStream = await TryInvoke(_repositories, async r =>
        {
            var ms = new MemoryStream();
            var resource = await r.GetResourceAsync<FindPackageByIdResource>(ct);

            // Download the package
            await resource.CopyNupkgToStreamAsync(
                packageIdentity.Id,
                packageIdentity.Version,
                ms,
                _cache,
                _nugetLogger,
                ct);

            source = r.PackageSource.Source;
            return ms;
        });

        packageStream.Seek(0, SeekOrigin.Begin);

        // Add it to the global package folder
        var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
            source,
            packageIdentity,
            packageStream,
            _globalPackagesFolder,
            parentId: Guid.Empty,
            ClientPolicyContext.GetClientPolicy(_settings, _nugetLogger),
            _nugetLogger,
            ct);

        if (downloadResult.Status != DownloadResourceResultStatus.Available)
            throw new Exception($"{nameof(downloadResult)} status is not Available: {downloadResult.Status}");

        return downloadResult.PackageReader;
    }

    private T TryInvoke<T>(IEnumerable<SourceRepository> repositories, Func<SourceRepository, T> func)
    {
        Exception? lastException = null;

        foreach (SourceRepository repository in repositories)
        {
            try
            {
                return func(repository);
            }
            catch (Exception e)
            {
                lastException = e;
                _logger.LogInformation($"An error occured while retrieving the resource from repository {repository}");
            }
        }

        throw lastException!;
    }

    public IEnumerator<SourceRepository> GetEnumerator()
    {
        return _repositories.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}