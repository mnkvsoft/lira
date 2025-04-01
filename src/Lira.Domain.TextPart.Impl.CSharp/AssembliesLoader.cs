using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Lira.Common;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.TextPart.Impl.CSharp;

class AssembliesLoader : IDisposable
{
    private readonly Dictionary<Hash, Assembly> _loadedAssembliesHashes = new();
    private readonly AssemblyLoadContext _context = new(name: null, isCollectible: true);
    private readonly CompilationStatistic _compilationStatistic;
    private readonly ILogger<AssembliesLoader> _logger;
    private readonly DynamicAssembliesUnloader _unLoader;
    private readonly Namer _namer;

    public AssembliesLoader(CompilationStatistic compilationStatistic, ILogger<AssembliesLoader> logger, DynamicAssembliesUnloader unLoader, Namer namer)
    {
        _compilationStatistic = compilationStatistic;
        _logger = logger;
        _unLoader = unLoader;
        _namer = namer;
    }

    public Assembly Load(PeImage peImage)
    {
        var sw = Stopwatch.StartNew();

        using var stream = new MemoryStream();
        stream.Write(peImage.Bytes.Value);
        stream.Position = 0;

        var hash = peImage.Hash;
        if (_loadedAssembliesHashes.TryGetValue(hash, out var assembly))
            return assembly;

        var result = _context.LoadFromStream(stream);

        _compilationStatistic.AddLoadAssemblyTime(sw.Elapsed);
        _loadedAssembliesHashes.Add(hash, result);

        return result;
    }

    public void Dispose()
    {
        var stat = _compilationStatistic;
        var nl = Constants.NewLine;
        _logger.LogDebug($"Dynamic csharp compilation statistic: " + nl +
                         $"Revision: {_namer.Revision}" + nl +
                         $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + nl +
                         $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + nl +
                         $"Count load assemblies: {stat.CountLoadAssemblies}." + nl +
                         $"Count functions: {stat.CountFunctionsTotal} (compile: {stat.CountFunctionsCompiled}, cache: {stat.CountFunctionsTotalFromCache})." + nl +
                         $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + nl);

        _unLoader.UnloadUnused(new DynamicAssembliesContext(_namer.Revision, _context));

        _logger.LogDebug("Count dynamic assemblies in current domain: " +
                         AppDomain.CurrentDomain
                             .GetAssemblies()
                             .Count(x => x.GetName().Name?.StartsWith(_namer.AssemblyPrefix) == true));
    }
}