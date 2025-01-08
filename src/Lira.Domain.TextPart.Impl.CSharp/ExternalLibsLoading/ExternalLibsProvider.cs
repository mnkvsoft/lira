using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using ArgValidation;
using Lira.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;

internal class ExternalLibsProvider
{
    private readonly ILogger<ExternalLibsProvider> _logger;
    private readonly NuGet.Common.ILogger _nugetLogger;
    private readonly string _libsPath;
    private readonly string _nugetConfigPath;
    private readonly string _rulesPath;

    public ExternalLibsProvider(IConfiguration configuration, ILogger<ExternalLibsProvider> logger)
    {
        _logger = logger;
        _nugetLogger = NullLogger.Instance;
        _rulesPath = configuration.GetRulesPath();
        _libsPath = configuration.GetLibsPath() ?? _rulesPath;
        _nugetConfigPath = configuration.GetNugetConfigPath() ?? _rulesPath;
    }

    private IReadOnlyCollection<string>? _dllLocations;

    public async Task<IReadOnlyCollection<string>> GetDllFilesLocations(CancellationToken ct = default)
    {
        if (_dllLocations != null)
            return _dllLocations;

        var packagesFile = Path.Combine(_rulesPath, "packages.json");

        if (!Path.Exists(packagesFile))
        {
            _dllLocations = Array.Empty<string>();
            return _dllLocations;
        }

        IReadOnlySet<PackageIdentity> packagesIdentities = await GetPackageIdentities(ct, packagesFile);

        if (packagesIdentities.Count == 0)
        {
            _dllLocations = Array.Empty<string>();
            return _dllLocations;
        }

        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(_nugetConfigPath))
        {
            _dllLocations = Array.Empty<string>();
            return _dllLocations;

            // basic implementation of nuget.config in code
            var setting = Settings.LoadSpecificSettings(_nugetConfigPath, "nuget.config");

            // get sources
            var packageSourceProvider = new PackageSourceProvider(setting);
            var sources = packageSourceProvider.LoadPackageSources();

            foreach (var source in sources)
            {
                Console.WriteLine($"{source.Name}: {source.SourceUri}");
            }
        }
        else
        {
            // Load machine and user settings
            var settings = Settings.LoadDefaultSettings(null);

            var repositories = GetSourceRepositories(settings);

            LogNugetInfo(settings, repositories);

            var cache = new SourceCacheContext();
            var packageIdentities = await GetPackagesWithDependencies(packagesIdentities, repositories, cache, ct);

            foreach (var packageIdentity in packageIdentities)
            {
                var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
                using var packageReader = await GetPackageReader(globalPackagesFolder, repositories, packageIdentity, cache, settings, ct);
                var supportedFrameworks = await packageReader.GetSupportedFrameworksAsync(ct);

                var selectedPackage = supportedFrameworks
                    .Where(x => x.Framework == ".NETCoreApp")
                    .OrderBy(x => x.Version)
                    .LastOrDefault();

                if(selectedPackage == null)
                    throw new Exception($"Unable to find compatible package for {packageIdentity.Id} {packageIdentity.Version}");

                var path = Path.Combine(
                    globalPackagesFolder,
                    packageIdentity.Id,
                    packageIdentity.Version.ToString(),
                    "lib",
                    selectedPackage.GetShortFolderName());

                result.AddRange(Directory.GetFiles(path, "*.dll"));
            }

            result.AddRange(Directory.GetFiles(_libsPath, "*.dll", SearchOption.AllDirectories));
            _dllLocations = result;
            return result;
        }
    }

    private void LogNugetInfo(ISettings settings, AvailableRepositories repositories)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nuget configs:");

        foreach (var path in settings.GetConfigFilePaths())
        {
            sb.AppendLine($" - {path}");
        }

        sb.AppendLine();
        sb.AppendLine("Nuget repositories:");
        foreach (var repository in repositories)
        {
            sb.AppendLine($" - {repository.PackageSource.Name}: {repository.PackageSource.Source}");
        }
        _logger.LogDebug(sb.ToString());
    }

    private async Task<PackageReaderBase> GetPackageReader(
        string globalPackagesFolder,
        AvailableRepositories repositories,
        PackageIdentity packageIdentity,
        SourceCacheContext cache,
        ISettings settings,
        CancellationToken ct)
    {
        var package = GlobalPackagesFolderUtility.GetPackage(packageIdentity, globalPackagesFolder);
        if (package != null)
            return package.PackageReader;

        using var packageStream = new MemoryStream();
        var (source, resource) = await repositories.TryInvoke(async r =>
        {
            var res = await r.GetResourceAsync<FindPackageByIdResource>(ct);
            return (r.PackageSource.Source, res);
        });

        // Download the package
        await resource.CopyNupkgToStreamAsync(
            packageIdentity.Id,
            packageIdentity.Version,
            packageStream,
            cache,
            _nugetLogger,
            ct);

        packageStream.Seek(0, SeekOrigin.Begin);

        // Add it to the global package folder
        var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
            source,
            packageIdentity,
            packageStream,
            globalPackagesFolder,
            parentId: Guid.Empty,
            ClientPolicyContext.GetClientPolicy(settings, _nugetLogger),
            _nugetLogger,
            ct);

        if(downloadResult.Status != DownloadResourceResultStatus.Available)
            throw new Exception($"{nameof(downloadResult)} status is not Available: {downloadResult.Status}");

        return downloadResult.PackageReader;
    }

    private async Task<HashSet<PackageIdentity>> GetPackagesWithDependencies(
        IReadOnlySet<PackageIdentity> packagesIdentities,
        AvailableRepositories repositories,
        SourceCacheContext cache,
        CancellationToken ct)
    {
        var allNeedPackages = await GetAllPackagesWithDependencies(packagesIdentities, repositories, cache, ct);

        var packageIdentities = new HashSet<PackageIdentity>();

        foreach (var packageIdentity in packagesIdentities)
        {
            // Find the best version for each package
            var resolverContext = new PackageResolverContext(
                dependencyBehavior: DependencyBehavior.Highest,
                targetIds: [packageIdentity.Id],
                requiredPackageIds: [],
                packagesConfig: [],
                preferredVersions: [],
                availablePackages: allNeedPackages,
                repositories.Select(r => r.PackageSource),
                _nugetLogger);

            var resolver = new PackageResolver();
            var resolvedPackages = resolver.Resolve(resolverContext, CancellationToken.None);

            foreach (var resolvedPackage in resolvedPackages)
            {
                packageIdentities.Add(resolvedPackage);
            }
        }

        return packageIdentities;
    }

    private AvailableRepositories GetSourceRepositories(ISettings settings)
    {
        var repositories = new List<SourceRepository>();
        foreach (var source in SettingsUtility.GetEnabledSources(settings))
        {
            repositories.Add(GetRepository(source));
        }

        return new AvailableRepositories(repositories, _logger);

        static SourceRepository GetRepository(PackageSource source)
        {
            return source.ProtocolVersion switch
            {
                2 => Repository.Factory.GetCoreV2(source),
                3 => Repository.Factory.GetCoreV3(source),
                _ => throw new Exception($"Unknown protocol version: {source.ProtocolVersion}")
            };
        }
    }

    private static async Task<IReadOnlySet<PackageIdentity>> GetPackageIdentities(CancellationToken ct,
        string packagesFile)
    {
        var packagesContent = await File.ReadAllTextAsync(packagesFile, ct);

        var packagesDto = JsonSerializer.Deserialize<Dictionary<string, string>>(packagesContent);

        if (packagesDto == null)
            return ImmutableHashSet<PackageIdentity>.Empty;

        var result = new HashSet<PackageIdentity>();
        foreach (var (id, versionStr) in packagesDto)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("package.config contains an empty package id");

            if (string.IsNullOrWhiteSpace(versionStr))
                throw new Exception($"The version for the package '{id}' is not declared");

            if (!NuGetVersion.TryParse(versionStr, out var version))
                throw new Exception($"The version for the package '{id}' is not a valid NuGet version");

            var packageId = new PackageIdentity(id, version);
            if (!result.Add(packageId))
                throw new Exception($"package.config contains duplicated package: {id}");
        }

        return result;
    }

    async Task<IReadOnlySet<SourcePackageDependencyInfo>> GetAllPackagesWithDependencies(
        IReadOnlyCollection<PackageIdentity> packagesIdentities,
        AvailableRepositories repositories,
        SourceCacheContext cache,
        CancellationToken ct)
    {
        var result = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

        foreach (var packageIdentity in packagesIdentities)
        {
            await ListAllPackageDependenciesRecursive(
                packageIdentity,
                repositories,
                cache,
                _nugetLogger,
                result,
                ct);
        }

        return result;
    }

    static async Task ListAllPackageDependenciesRecursive(
        PackageIdentity package,
        AvailableRepositories repositories,
        SourceCacheContext cache,
        NuGet.Common.ILogger logger,
        HashSet<SourcePackageDependencyInfo> dependencies,
        CancellationToken ct)
    {
        if (dependencies.Contains(package))
            return;

        var dependencyInfoResource = await repositories.TryInvoke(r => r.GetResourceAsync<DependencyInfoResource>(ct));
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, GetCurrentFramework(), cache, logger, ct);

        if (dependencyInfo == null)
            return;

        if (dependencies.Add(dependencyInfo))
        {
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                await ListAllPackageDependenciesRecursive(
                    new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                    repositories,
                    cache,
                    logger,
                    dependencies,
                    ct);
            }
        }
    }

    private static NuGetFramework GetCurrentFramework()
    {
        var targetFrameworkAttribute = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<TargetFrameworkAttribute>();

        return NuGetFramework.Parse(
            targetFrameworkAttribute?.FrameworkName ??
            throw new Exception("Cannot determine the current version of .net")
        );
    }

    class AvailableRepositories : IEnumerable<SourceRepository>
    {
        private readonly IReadOnlyCollection<SourceRepository> _repositories;
        private readonly ILogger _logger;

        public AvailableRepositories(IReadOnlyCollection<SourceRepository> repositories, ILogger logger)
        {
            Arg.NotEmpty(repositories, nameof(repositories));

            _repositories = repositories;
            _logger = logger;
        }

        public T TryInvoke<T>(Func<SourceRepository, T> func)
        {
            Exception? lastException = null;

            foreach (SourceRepository repository in _repositories)
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
}