using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;
using Lira.Common;
using Lira.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;

internal class ExternalLibsProvider
{
    private static readonly string LibsNugetPath = Paths.GetTempSubPath("libs_nuget");

    private ILogger<ExternalLibsProvider> _logger;
    private readonly string _libsPath;
    private readonly string _nugetConfigPath;
    private readonly string _rulesPath;

    public ExternalLibsProvider(IConfiguration configuration, ILogger<ExternalLibsProvider> logger)
    {
        _logger = logger;
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

        var packagesContent = await File.ReadAllTextAsync(packagesFile, ct);
        var packages = JsonSerializer.Deserialize<Dictionary<string, string>>(packagesContent);

        if (packages == null || packages.Count == 0)
        {
            _dllLocations = Array.Empty<string>();
            return _dllLocations;
        }

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
            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(ct);

            foreach (var (id, version) in packages)
            {
                if (string.IsNullOrWhiteSpace(version))
                    throw new Exception($"The version for the package '{id}' is not declared");

                var packageVersion = new NuGetVersion(version);
                using var packageStream = new MemoryStream();

                await resource.CopyNupkgToStreamAsync(
                    id,
                    packageVersion,
                    packageStream,
                    cache,
                    NullLogger.Instance,
                    ct);

                _logger.LogInformation($"Downloaded package {id} {packageVersion}");

                using var packageReader = new PackageArchiveReader(packageStream);

                var nuspecReader = await packageReader.GetNuspecReaderAsync(ct);
                var supportedFrameworks = (await packageReader.GetSupportedFrameworksAsync(ct)).ToArray();

                var currentVersion = GetCurrentFramework();
                var selectedPackage = supportedFrameworks
                    .Last(lib => DefaultCompatibilityProvider.Instance.IsCompatible(
                        currentVersion,
                        lib)
                    );

                var netVersion = Environment.Version;

                var cur = GetCurrentFramework();

                var a = packageReader.GetFiles().ToArray();

                _logger.LogInformation($"Description: {nuspecReader.GetDescription()}");
            }



            var result = new List<string>();
            result.AddRange(Directory.GetFiles(_libsPath, "*.dll", SearchOption.AllDirectories));
            _dllLocations = result;
            return result;
        }


    }

    private static NuGetFramework GetCurrentFramework()
    {
        var targetFrameworkAttribute = Assembly
            .GetExecutingAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>();

        return NuGetFramework.Parse(
            targetFrameworkAttribute?.FrameworkName ?? throw new Exception("Cannot determine the current version of .net")
        );
    }
}