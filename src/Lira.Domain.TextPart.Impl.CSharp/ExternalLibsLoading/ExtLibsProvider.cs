using Lira.Configuration;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading.Nuget;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;

internal class ExtLibsProvider
{
    private readonly string _libsPath;
    private readonly ILogger<ExtLibsProvider> _logger;
    private readonly NugetLibsProvider _nugetLibsProvider;

    public ExtLibsProvider(IConfiguration configuration, ILogger<ExtLibsProvider> logger, NugetLibsProvider nugetLibsProvider)
    {
        _logger = logger;
        _nugetLibsProvider = nugetLibsProvider;
        _libsPath = configuration.GetLibsPath() ?? configuration.GetRulesPath();
    }

    public async Task<IReadOnlyCollection<string>> GetLibsFiles(CancellationToken ct = default)
    {
        var libs = Directory.GetFiles(_libsPath, "*.dll", SearchOption.AllDirectories);
        var nugetLibs = await _nugetLibsProvider.GetLibsFiles(ct);

        _logger.LogDebug(
            $"Loaded libs.\n" +
            $"Directly:\n{string.Join("\n", libs.Select(l => " - " + l))}\n" +
            $"Nuget:\n{string.Join("\n", nugetLibs.Select(l => " - " + l))}");

        return libs.Union(nugetLibs).ToArray();
    }
}