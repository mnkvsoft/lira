using System.Collections.Immutable;
using Lira.Common;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;
using Lira.Domain.TextPart.Types;
using Newtonsoft.Json.Linq;

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactoryCreator
{
    private readonly ExtLibsProvider _extLibsProvider;
    private readonly FunctionFactory.Dependencies _functionFactoryDependencies;
    private readonly CsFilesCompiler.Dependencies _csFilesCompilerDependencies;
    private FunctionFactory? _factory;


    public FunctionFactoryCreator(FunctionFactory.Dependencies functionFactoryFunctionFactoryDependencies, CsFilesCompiler.Dependencies csFilesCompilerDependencies, ExtLibsProvider extLibsProvider)
    {
        _extLibsProvider = extLibsProvider;
        _functionFactoryDependencies = functionFactoryFunctionFactoryDependencies;
        _csFilesCompilerDependencies = csFilesCompilerDependencies;
    }

    public async Task<FunctionFactory> Create()
    {
        if(_factory != null)
           return _factory;

        var systemLibs = SystemAssemblies.Locations;

        var projectLibs = new[]
        {
            typeof(IObjectTextPart).Assembly.Location,
            typeof(IRangesProvider).Assembly.Location,
            typeof(Constants).Assembly.Location,
            typeof(Json).Assembly.Location,
            typeof(JObject).Assembly.Location,
            typeof(RequestData).Assembly.Location,
            GetType().Assembly.Location
        };

        var externalLibs = await _extLibsProvider.GetLibsFiles();

        var allLibs = systemLibs.Concat(projectLibs).Concat(externalLibs).ToImmutableList();

        var csFilesCompiler = new CsFilesCompiler(_csFilesCompilerDependencies, allLibs);

        var csFilesAssembly = await csFilesCompiler.GetCsFilesAssembly();
        _factory = new FunctionFactory(_functionFactoryDependencies, allLibs, csFilesAssembly);
        return _factory;
    }
}