using System.Diagnostics;
using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

abstract record CompileResult
{
    public record Success(PeImage PeImage) : CompileResult;
    public record Fault(string Message) : CompileResult;
}

record CompileUnit(IReadOnlyCollection<string> Codes, string AssemblyName, UsageAssemblies? UsageAssemblies);

class Compiler
{
    private readonly CompilationStatistic _compilationStatistic;
    private readonly PeImagesCache _peImagesCache;

    public Compiler(CompilationStatistic compilationStatistic, PeImagesCache peImagesCache)
    {
        _compilationStatistic = compilationStatistic;
        _peImagesCache = peImagesCache;
    }

    public CompileResult Compile(CompileUnit compileUnit)
    {
        var sw = Stopwatch.StartNew();

        var hash = GetHash(compileUnit);

        if (_peImagesCache.TryGet(hash, out var peImage))
            return new CompileResult.Success(peImage);

        var compileResult = CodeCompiler.Compile(compileUnit.Codes, compileUnit.AssemblyName, compileUnit.UsageAssemblies);

        if (compileResult is CompileResult.Success success)
        {
            _peImagesCache.Add(hash, success.PeImage);
            _compilationStatistic.AddCompilationTime(sw.Elapsed);
        }

        return compileResult;
    }

    private Hash GetHash(CompileUnit compileUnit)
    {
        using var memoryStream = new MemoryStream();
        using var sw = new StreamWriter(memoryStream);

        sw.Write(string.Concat(compileUnit.Codes));

        var usageAssemblies = compileUnit.UsageAssemblies;
        if (usageAssemblies != null)
        {
            foreach (var peImage in usageAssemblies.Runtime)
            {
                sw.Write(GetHash(peImage));
            }

            foreach (var assembly in usageAssemblies.Compiled)
            {
                sw.Write(assembly.FullName);
            }
        }

        sw.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var hash = Sha1.Create(memoryStream);
        return hash;
    }

    private static Hash GetHash(PeImage image) => Sha1.Create(image.Bytes);
}
