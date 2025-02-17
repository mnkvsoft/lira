using System.Diagnostics;
using System.Text;
using Lira.Common;
using Lira.Common.Exceptions;
using Microsoft.CodeAnalysis;
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

        _compilationStatistic.AddFunctionTotal();
        if (_peImagesCache.TryGet(hash, out var peImage))
        {
            _compilationStatistic.AddFunctionFromCache();
            return new CompileResult.Success(peImage);
        }

        var compileResult = CodeCompiler.Compile(
            compileUnit.AssemblyName,
            GetMetadataReferences(compileUnit.References),
            compileUnit.Codes);

        if (compileResult is CodeCompiler.Result.Success success)
        {
            var image = new PeImage(hash, success.PeBytes);
            _peImagesCache.Add(image);
            _compilationStatistic.AddCompilationTime(sw.Elapsed);
            return new CompileResult.Success(image);
        }

        if (compileResult is CodeCompiler.Result.Fault fail)
        {
            return new CompileResult.Fault(fail.Message);
        }

        throw new UnsupportedInstanceType(compileResult);
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

        sb.AppendLine("Codes: " + Sha1.Create(string.Concat(compileUnit.Codes)));

        var usageAssemblies = compileUnit.References;
        foreach (var peImage in usageAssemblies.Runtime)
        {
            sw.Write(peImage.Hash);
            sb.AppendLine("Runtimes: " + peImage.Hash);
        }

        foreach (var location in usageAssemblies.AssembliesLocations)
        {
            sw.Write(location);
        }

        sb.AppendLine("AssembliesLocations: " + Sha1.Create(string.Concat(usageAssemblies.AssembliesLocations)));

        sw.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var hash = Sha1.Create(memoryStream);

        sb.AppendLine("Result: " + hash);
        sb.AppendLine("-------- end Compile Unit --------");
        _logger.LogTrace(sb.ToString());

        return hash;
    }

    private static IReadOnlyCollection<MetadataReference> GetMetadataReferences(References references)
    {
        var result = new List<MetadataReference>();

        result.AddRange(references.AssembliesLocations.Select(dllPath => MetadataReference.CreateFromFile(dllPath)));
        result.AddRange(references.Runtime.Select(peImage => MetadataReference.CreateFromImage(peImage.Bytes.Value)));

        return result;
    }
}