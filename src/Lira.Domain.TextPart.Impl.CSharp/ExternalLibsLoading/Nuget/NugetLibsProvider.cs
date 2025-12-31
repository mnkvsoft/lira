using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Lira.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading.Nuget;

internal class NugetLibsProvider
{
    private readonly ILogger<NugetLibsProvider> _logger;
    private readonly NuGet.Common.ILogger _nugetLogger;

    private readonly string _rulesPath;

    private readonly AvailableRepositories _repositories;
    private readonly NuGetFramework _currentFramework;

    private readonly Dictionary<string, IReadOnlyCollection<string>> _packagesConfigToLibsCache = new();

    public NugetLibsProvider(IConfiguration configuration, IRulesPathProvider rulesPathProvider, ILogger<NugetLibsProvider> logger)
    {
        _logger = logger;
        _nugetLogger = NullLogger.Instance;
        _rulesPath = rulesPathProvider.Path;

        // Load machine and user settings

        var settings = GetSettings(_rulesPath, configuration.GetNugetConfigPath());
        _repositories = new AvailableRepositories(settings, _logger, _nugetLogger);
        LogNugetInfo(settings, _repositories);

        _currentFramework = GetCurrentFramework();
    }

    private static ISettings GetSettings(string libsPath, string? nugetConfigPath)
    {
        if (!string.IsNullOrWhiteSpace(nugetConfigPath))
        {
            if (!File.Exists(nugetConfigPath))
                throw new Exception($"Nuget config file not found: {nugetConfigPath}");

            return Settings.LoadSpecificSettings(
                Path.GetDirectoryName(nugetConfigPath)!,
                Path.GetFileName(nugetConfigPath));
        }

        var nugetConfigs = Directory.GetFiles(libsPath, "nuget.config", SearchOption.AllDirectories);

        if (nugetConfigs.Length > 1)
            throw new Exception(
                $"More than one nuget.config file found in {libsPath}\n{string.Join("\n - ", nugetConfigs)}");

        if (nugetConfigs.Length == 1)
        {
            return Settings.LoadSpecificSettings(
                Path.GetDirectoryName(nugetConfigs[0])!,
                Path.GetFileName(nugetConfigs[0]));
        }

        return Settings.LoadDefaultSettings(null);
    }

    public async Task<IReadOnlyCollection<string>> GetLibsFiles(CancellationToken ct = default)
    {
        var empty = Array.Empty<string>();

        var packagesFile = Path.Combine(_rulesPath, "packages.json");

        if (!Path.Exists(packagesFile))
            return empty;

        var packagesContent = await File.ReadAllTextAsync(packagesFile, ct);
        if (_packagesConfigToLibsCache.TryGetValue(packagesContent, out var libsPaths))
        {
            _logger.LogDebug("Nuget libs paths loaded from memory cache");
            return libsPaths;
        }

        var packagesIdentities = GetPackageIdentities(packagesContent);

        if (packagesIdentities.Count == 0)
            return empty;

        var packageIdentities = (await GetPackagesWithDependencies(packagesIdentities, ct))
            .Where(identity => identity.Id != "Microsoft.NETCore.Platforms" && identity.Id != "NETStandard.Library")
            .ToArray();

        var result = new List<string>();

        foreach (var packageIdentity in packageIdentities)
        {
            using PackageReaderBase packageReader = await _repositories.GetPackageReader(packageIdentity, ct);

            var supportedFrameworks = (await packageReader.GetSupportedFrameworksAsync(ct)).ToArray();

            var selectedFramework = NuGetFrameworkUtility.GetNearest(
                supportedFrameworks,
                _currentFramework,
                framework => framework);

            if (selectedFramework == null)
            {
                throw new Exception(
                    $"Unable to find compatible package for {packageIdentity.Id} {packageIdentity.Version}." + Environment.NewLine +
                    $"Current framework: {_currentFramework}." + Environment.NewLine +
                    $"Supported frameworks: " + string.Join(", ", supportedFrameworks.Select(x => x.Framework + " " + x.Version)));
            }

            var nuspecFile = await packageReader.GetNuspecFileAsync(ct);
            var packageDirectory = Path.GetDirectoryName(nuspecFile) ??
                                   throw new Exception($"Unable to find directory for {nuspecFile}");

            var libItems = (await packageReader.GetLibItemsAsync(ct));
            var libPath = libItems.Single(x => x.TargetFramework == selectedFramework);

            var dllPaths = libPath.Items
                .Where(x => x.EndsWith(".dll"))
                .Select(dllPath => Path.Combine(packageDirectory, dllPath));

            result.AddRange(dllPaths);
        }

        _logger.LogDebug("Nuget packages with dependencies:" + Environment.NewLine +
                               string.Join(Environment.NewLine, packageIdentities.Select(package => $" - {package}")));

        _packagesConfigToLibsCache.Add(packagesContent, result.ToArray());

        return result;
    }

    private void LogNugetInfo(ISettings settings, AvailableRepositories repositories)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nuget configuration:");
        sb.AppendLine();
        sb.AppendLine("Configs:");

        foreach (var path in settings.GetConfigFilePaths())
        {
            sb.AppendLine($" - {path}");
        }

        sb.AppendLine();
        sb.AppendLine("Repositories:");
        foreach (var repository in repositories)
        {
            sb.AppendLine($" - {repository.PackageSource.Name}: {repository.PackageSource.Source}");
        }

        sb.AppendLine();
        sb.AppendLine("Global package folder:");
        sb.AppendLine(SettingsUtility.GetGlobalPackagesFolder(settings));

        _logger.LogDebug(sb.ToString());
    }

    private async Task<HashSet<PackageIdentity>> GetPackagesWithDependencies(
        IReadOnlySet<PackageIdentity> packagesIdentities,
        CancellationToken ct)
    {
        var allNeedPackages = await GetAllPackagesWithDependencies(packagesIdentities, ct);

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
                _repositories.Select(r => r.PackageSource),
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

    private static IReadOnlySet<PackageIdentity> GetPackageIdentities(string packagesContent)
    {
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
        CancellationToken ct)
    {
        var result = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

        foreach (var packageIdentity in packagesIdentities)
        {
            await ListAllPackageDependenciesRecursive(
                packageIdentity,
                result,
                ct);
        }

        return result;
    }

    async Task ListAllPackageDependenciesRecursive(
        PackageIdentity package,
        HashSet<SourcePackageDependencyInfo> dependencies,
        CancellationToken ct)
    {
        if (dependencies.Contains(package))
            return;

        var dependencyInfo = await _repositories.ResolvePackage(package, _currentFramework, ct);

        if (dependencyInfo == null)
            return;

        if (dependencies.Add(dependencyInfo))
        {
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                await ListAllPackageDependenciesRecursive(
                    new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
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
}