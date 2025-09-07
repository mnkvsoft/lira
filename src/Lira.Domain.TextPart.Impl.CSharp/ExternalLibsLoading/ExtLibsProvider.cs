using System.Collections.Immutable;
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

    public ExtLibsProvider(IConfiguration configuration, IRulesPathProvider rulesPathProvider, ILogger<ExtLibsProvider> logger, NugetLibsProvider nugetLibsProvider)
    {
        _logger = logger;
        _nugetLibsProvider = nugetLibsProvider;
        _libsPath = configuration.GetLibsPath() ?? rulesPathProvider.Path;
    }

    public async Task<IImmutableList<string>> GetLibsFiles(CancellationToken ct = default)
    {
        var libs = Directory.GetFiles(_libsPath, "*.dll", SearchOption.AllDirectories);
        var nugetLibs = await _nugetLibsProvider.GetLibsFiles(ct);

        var nl = Environment.NewLine;
        _logger.LogDebug(
            $"Loaded libs:" + nl + nl +
            $"Nuget:" + nl + $"{string.Join(nl, nugetLibs.Select(l => " - " + l))}" + nl + nl +
            $"Directly:{ (libs.Length == 0 ? " none" : "\n" + string.Join("\n", libs.Select(l => " - " + l)))}");

        return libs.Union(nugetLibs).ToImmutableArray();
    }
}