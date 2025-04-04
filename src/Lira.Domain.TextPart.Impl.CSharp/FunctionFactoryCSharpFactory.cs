using System.Collections.Immutable;
using Lira.Common;
using Lira.Domain.DataModel;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.ExternalLibsLoading;
using Lira.Domain.TextPart.Types;
using Newtonsoft.Json.Linq;

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactoryCSharpFactory : IFunctionFactoryCSharpFactory
{
    private readonly ExtLibsProvider _extLibsProvider;
    private readonly FunctionFactory.Dependencies _functionFactoryDependencies;
    private readonly CsFilesCompiler.Dependencies _csFilesCompilerDependencies;
    private FunctionFactory? _factory;
    private readonly AssembliesLoader _assembliesLoader;

    public FunctionFactoryCSharpFactory(
        FunctionFactory.Dependencies functionFactoryFunctionFactoryDependencies,
        CsFilesCompiler.Dependencies csFilesCompilerDependencies,
        ExtLibsProvider extLibsProvider,
        AssembliesLoader assembliesLoader)
    {
        _extLibsProvider = extLibsProvider;
        _assembliesLoader = assembliesLoader;
        _functionFactoryDependencies = functionFactoryFunctionFactoryDependencies;
        _csFilesCompilerDependencies = csFilesCompilerDependencies;
    }

    public async Task<IFunctionFactoryCSharp> Get()
    {
        if (_factory != null)
            return _factory;

        // these assemblies already loaded to app domain
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

        var systemLibs = SystemAssemblies.Locations;
        foreach (var libLocation in systemLibs)
        {
            if (AppDomain.CurrentDomain.GetAssemblies().All(assembly => assembly.Location != libLocation))
            {
                _assembliesLoader.Load(libLocation);
            }
        }

        var externalLibs = await _extLibsProvider.GetLibsFiles();
        foreach (var libLocation in externalLibs)
        {
            _assembliesLoader.Load(libLocation);
        }

        var allLibs = systemLibs.Concat(projectLibs).Concat(externalLibs).ToImmutableList();

        var csFilesCompiler = new CsFilesCompiler(_csFilesCompilerDependencies, allLibs);

        var csFilesAssembly = await csFilesCompiler.GetCsFilesAssembly();
        _factory = new FunctionFactory(_functionFactoryDependencies, allLibs, csFilesAssembly);
        return _factory;
    }
}