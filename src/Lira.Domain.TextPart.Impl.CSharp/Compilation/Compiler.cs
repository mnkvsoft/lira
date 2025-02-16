using System.Diagnostics;
using System.Text;
using Lira.Common;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

class Compiler
{
    private readonly CompilationStatistic _compilationStatistic;
    private readonly PeImagesCache _peImagesCache;
    private readonly ILogger<Compiler> _logger;

    public Compiler(CompilationStatistic compilationStatistic, PeImagesCache peImagesCache, ILogger<Compiler> logger)
    {
        _compilationStatistic = compilationStatistic;
        _peImagesCache = peImagesCache;
        _logger = logger;
    }

    public CompileResult Compile(CompileUnit compileUnit)
    {
        var sw = Stopwatch.StartNew();

        var hash = GetHash(compileUnit);

        if (_peImagesCache.TryGet(hash, out var peImage))
            return new CompileResult.Success(peImage);

        var compileResult = CodeCompiler.Compile(compileUnit);

        if (compileResult is CompileResult.Success success)
        {
            _peImagesCache.Add(hash, success.PeImage);
            _compilationStatistic.AddCompilationTime(sw.Elapsed);
        }

        return compileResult;
    }

    private Hash GetHash(CompileUnit compileUnit)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---------- Compile Unit ----------");

        using var memoryStream = new MemoryStream();
        using var sw = new StreamWriter(memoryStream);

        sw.Write(compileUnit.AssemblyName);
        sb.AppendLine("Assembly name: " + compileUnit.AssemblyName);

        foreach (var code in compileUnit.Codes)
        {
            sw.Write(code);
        }

        sb.AppendLine("Codes:");
        sb.AppendJoin("\n\n", compileUnit.Codes);

        var usageAssemblies = compileUnit.UsageAssemblies;
        if (usageAssemblies != null)
        {
            foreach (var peImage in usageAssemblies.Runtime)
            {
                sw.Write(Sha1.Create(peImage.Bytes));

                sb.AppendLine("Runtime: " + Sha1.Create(peImage.Bytes));
            }

            foreach (var location in usageAssemblies.AssembliesLocations)
            {
                sw.Write(location);
            }

            sb.AppendLine("AssembliesLocations: " + Sha1.Create(string.Concat(usageAssemblies.AssembliesLocations)));
        }


        sw.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var hash = Sha1.Create(memoryStream);

        sb.AppendLine("Result: " + hash);
        sb.AppendLine("-------- end Compile Unit --------");
        _logger.LogTrace(sb.ToString());

        return hash;
    }
}
