using System.Collections.Immutable;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Configuration;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Microsoft.Extensions.Configuration;

namespace Lira.Domain.TextPart.Impl.CSharp;

class CsFilesCompiler
{
    public record Dependencies(
        IConfiguration Configuration,
        Compiler Compiler,
        AssembliesLoader AssembliesLoader,
        Namer Namer);

    private readonly string _path;

    private readonly Compiler _compiler;
    private readonly AssembliesLoader _assembliesLoader;
    private readonly IImmutableList<string> _assembliesLocations;
    private readonly Namer _namer;

    public CsFilesCompiler(Dependencies dependencies, IImmutableList<string> assembliesLocations)
    {
        _compiler = dependencies.Compiler;
        _assembliesLoader = dependencies.AssembliesLoader;
        _namer = dependencies.Namer;
        _path = dependencies.Configuration.GetRulesPath();

        _assembliesLocations = assembliesLocations;
    }

    public async Task<CsFilesAssembly?> GetCsFilesAssembly()
    {
        var csharpFiles = Directory.GetFiles(_path, "*.cs", SearchOption.AllDirectories)
            .Where(x => x != Consts.GlobalUsingsRulesFileName)
            .ToArray();

        if (csharpFiles.Length == 0)
            return null;

        var codes = await Task.WhenAll(csharpFiles.Select(async x => await File.ReadAllTextAsync(x)));

        var compileResult = _compiler.Compile(
            new CompileUnit(
                _namer.GetAssemblyName(_namer.GetClassName("CustomType", codes)),
                codes.ToImmutableArray(),
                new References(
                    Runtime: Array.Empty<PeImage>(),
                    AssembliesLocations: _assembliesLocations)));

        var nl = Constants.NewLine;

        if (compileResult is CompileResult.Fault fault)
        {
            throw new Exception("An error occurred while compile C# files. " + fault.Message + nl +
                                "Files:" + nl + nl +
                                csharpFiles.JoinWithNewLine());
        }

        var peImage = ((CompileResult.Success)compileResult).PeImage;

        var assembly = _assembliesLoader.Load(peImage);

        return new CsFilesAssembly(assembly, peImage);
    }
}