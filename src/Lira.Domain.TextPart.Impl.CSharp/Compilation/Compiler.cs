using System.Diagnostics;
using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

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

        var hash = compileUnit.GetHash();

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
}
