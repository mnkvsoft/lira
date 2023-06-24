using System.Diagnostics;
using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.CSharp.Compilation;

record CompileUnit(string Code, string AssemblyName, UsageAssemblies? UsageAssemblies);

class Compiler
{
    private readonly CompilationStatistic _compilationStatistic;
    private readonly PeImagesCache _peImagesCache;

    public Compiler(CompilationStatistic compilationStatistic, PeImagesCache peImagesCache)
    {
        _compilationStatistic = compilationStatistic;
        _peImagesCache = peImagesCache;
    }

    public PeImage Compile(CompileUnit compileUnit)
    {
        var hash = GetHash(compileUnit);

        if (_peImagesCache.TryGet(hash, out var peImage))
            return peImage;

        var sw = Stopwatch.StartNew();

        peImage = CodeCompiler.Compile(new[] { compileUnit.Code }, compileUnit.AssemblyName, compileUnit.UsageAssemblies);

        _peImagesCache.Add(hash, peImage);
        
        _compilationStatistic.AddCompilationTime(sw.Elapsed);

        return peImage;
    }

    private Hash GetHash(CompileUnit compileUnit)
    {
        using var memoryStream = new MemoryStream();
        using var sw = new StreamWriter(memoryStream);

        sw.Write(compileUnit.Code);

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
