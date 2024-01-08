using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Lira.Common;
using Lira.Configuration;
using Lira.Domain.Actions;
using Lira.Domain.Matching.Request;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.TextPart.Types;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactory : IFunctionFactoryCSharp
{
    private static int _revisionCounter;

    private readonly ILogger _logger;
    private readonly string _path;

    private readonly AssemblyLoadContext _context = new(name: null, isCollectible: true);
    private readonly int _revision;

    private const string AssemblyPrefix = "__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{_revision}_{name}";

    private readonly CompilationStatistic _compilationStatistic;
    private readonly DynamicAssembliesUnloader _unLoader;
    private readonly Compiler _compiler;
    private readonly Cache _cache;

    public FunctionFactory(IConfiguration configuration, ILoggerFactory loggerFactory, DynamicAssembliesUnloader unLoader,
        Compiler compiler, CompilationStatistic compilationStatistic, Cache cache)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();
        _revision = ++_revisionCounter;
        _unLoader = unLoader;
        _compiler = compiler;
        _compilationStatistic = compilationStatistic;
        _cache = cache;
    }

    public CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "GeneratingFunction";
        var customAssemblies = GetCustomAssemblies();
        var className = GetClassName(prefix, code);

        var classToCompile = CreateGeneratingFunctionClassCode(className, customAssemblies.Loaded, declaredPartsProvider, code);

        var result = CreateFunctionResult<IObjectTextPart>(
            assemblyPrefixName: prefix,
            declaredPartsProvider,
            classToCompile,
            className,
            customAssemblies.PeImages);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "TransformFunction";
        string className = GetClassName(prefix, code);
        var customAssemblies = GetCustomAssemblies();

        string classToCompile = ClassCodeCreator.CreateTransformFunction(
            className,
            WrapToTryCatch(new Code(ForCompile: $"return {code};", Source: code)),
            ReservedVariable.Value,
            GetNamespaces(customAssemblies.Loaded),
            GetUsingStatic(customAssemblies.Loaded));

        var result = CreateFunctionResult<ITransformFunction>(
            assemblyPrefixName: prefix,
            declaredPartsProvider,
            classToCompile,
            className,
            customAssemblies.PeImages);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IMatchFunction> TryCreateMatchFunction(IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "MatchFunction";
        string className = GetClassName(prefix, code);
        var customAssemblies = GetCustomAssemblies();

        string classToCompile = ClassCodeCreator.CreateMatchFunction(
            className,
            GetMethodBody(new Code(ForCompile: code, Source: code)),
            ReservedVariable.Value,
            GetNamespaces(customAssemblies.Loaded),
            GetUsingStatic(customAssemblies.Loaded));

        var result = CreateFunctionResult<IMatchFunction>(
            assemblyPrefixName: prefix,
            declaredPartsProvider,
            classToCompile,
            className,
            customAssemblies.PeImages);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IAction> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "Action";
        string className = GetClassName(prefix, code);
        var customAssemblies = GetCustomAssemblies();

        string classToCompile = CreateActionClassCode(className, customAssemblies.Loaded, declaredPartsProvider, code);

        var result = CreateFunctionResult<IAction>(
            assemblyPrefixName: prefix,
            declaredPartsProvider,
            classToCompile,
            className,
            customAssemblies.PeImages);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    private CreateFunctionResult<TFunction> CreateFunctionResult<TFunction>(
        string assemblyPrefixName,
        IDeclaredPartsProvider declaredPartsProvider,
        string classToCompile,
        string className,
        IReadOnlyCollection<PeImage> peImages)
    {
        PeImage peImage;

        try
        {
            peImage = _compiler.Compile(
                new CompileUnit(
                    classToCompile,
                    AssemblyName: GetAssemblyName(assemblyPrefixName + className),
                    new UsageAssemblies(
                        Compiled: new Assembly[]
                        {
                            typeof(IObjectTextPart).Assembly,
                            typeof(Json).Assembly,
                            typeof(RequestData).Assembly,
                            GetType().Assembly
                        },
                        Runtime: peImages)));
        }
        catch (Exception e)
        {
            return new CreateFunctionResult<TFunction>.Failed(e);
        }

        var classAssembly = Load(peImage);

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        var function = (TFunction)Activator.CreateInstance(type, new DynamicObjectBase.Dependencies(declaredPartsProvider, _cache))!;

        return new CreateFunctionResult<TFunction>.Success(function);
    }

    private bool _customAssembliesWasInit;
    private IReadOnlyCollection<Assembly> _customLoadedAssemblies = null!;
    private IReadOnlyCollection<PeImage> _customPeImageAssemblies = null!;

    private (IReadOnlyCollection<Assembly> Loaded, IReadOnlyCollection<PeImage> PeImages) GetCustomAssemblies()
    {
        if (_customAssembliesWasInit)
            return (_customLoadedAssemblies, _customPeImageAssemblies);

        var customAssemblies = GetCustomAssemblies(_path);
        _customAssembliesWasInit = true;

        _customLoadedAssemblies = customAssemblies.Select(x => x.LoadedAssembly).ToArray();
        _customPeImageAssemblies = customAssemblies.Select(x => x.PeImage).ToArray();

        return (_customLoadedAssemblies, _customPeImageAssemblies);
    }

    private IReadOnlyCollection<CustomAssembly> GetCustomAssemblies(string path)
    {
        var csharpFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        if (csharpFiles.Length == 0)
            return Array.Empty<CustomAssembly>();

        var codes = csharpFiles.Select(File.ReadAllText).ToList();

        var result = new List<CustomAssembly>();

        foreach (var code in codes)
        {
            var peImage = _compiler.Compile(new CompileUnit(code, GetAssemblyName(GetClassName("CustomType", code)),
                UsageAssemblies: null));
            var assembly = Load(peImage);
            result.Add(new CustomAssembly(assembly, peImage));
        }

        return result;
    }

    private Assembly Load(PeImage peImage)
    {
        var sw = Stopwatch.StartNew();

        using var stream = new MemoryStream();
        stream.Write(peImage.Bytes);
        stream.Position = 0;
        var result = _context.LoadFromStream(stream);

        _compilationStatistic.AddLoadAssemblyTime(sw.Elapsed);

        return result;
    }

    private static string CreateGeneratingFunctionClassCode(string className, IReadOnlyCollection<Assembly> customAssemblies,
        IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        const string requestParameterName = "_request_";
        const string repeatFunctionName = "repeat";

        string classToCompile = ClassCodeCreator.CreateIObjectTextPart(
            className,
            GetMethodBody(new Code(
                ForCompile: 
                code.StartsWith(repeatFunctionName) 
                ? ReplaceVariableNamesForRepeat(code, declaredPartsProvider)
                : ReplaceVariableNames(code, declaredPartsProvider, requestParameterName), 
                Source: code)),
            requestParameterName,
            ReservedVariable.Req,
            repeatFunctionName,
            GetNamespaces(customAssemblies),
            GetUsingStatic(customAssemblies));

        return classToCompile;
    }

    private static string CreateActionClassCode(
        string className,
        IReadOnlyCollection<Assembly> customAssemblies,
        IDeclaredPartsProvider declaredPartsProvider,
        string code)
    {
        const string requestParameterName = "_request_";

        string classToCompile = ClassCodeCreator.CreateAction(
            className,
            WrapToTryCatch(new Code(
                ForCompile: ReplaceVariableNames(code, declaredPartsProvider, requestParameterName) + ";", 
                Source: code)),
            requestParameterName,
            ReservedVariable.Req,
            GetNamespaces(customAssemblies),
            GetUsingStatic(customAssemblies));

        return classToCompile;
    }

    private static string[] GetNamespaces(IReadOnlyCollection<Assembly> customAssemblies)
    {
        return customAssemblies.SelectMany(a => a.GetTypes())
            .Where(x => x.IsVisible && x.Namespace?.StartsWith("_") == true)
            .Select(t => t.Namespace!)
            .Distinct()
            .ToArray();
    }

    private static string[] GetUsingStatic(IReadOnlyCollection<Assembly> customAssemblies)
    {
        return customAssemblies.SelectMany(a => a.GetTypes())
            .Where(x => x.IsVisible && x.Name.StartsWith("_"))
            .Select(t => t.FullName!)
            .Distinct()
            .ToArray();
    }

    record Code(string ForCompile, string Source);

    private static string GetMethodBody(Code code)
    {
        string methodBody;

        if (code.ForCompile.Contains("return"))
        {
            methodBody = WrapToTryCatch(code);
        }
        else
        {
            methodBody = WrapToTryCatch(
                code with
                {
                    ForCompile = $"var _result_ = {code.ForCompile};" + Constants.NewLine +
                                 @"; return _result_;"
                });
        }

        return methodBody;
    }

    private static string WrapToTryCatch(Code code)
    {
        return @"try
            {
                " + code.ForCompile + @"
            }
            catch (Exception e)
            {
                var nl = NewLine;
                throw new Exception(" + "\"An error has occurred while execute code block: \" + nl + \"" +
               code.Source
                   .Replace("\"", "\\\"")
                   .Replace("\r\n", "\" + nl + \"")
                   .Replace("\n", "\" + nl + \"") +
               "\", e);" + @"
            }";
    }

    private static string ReplaceVariableNames(string code, IDeclaredPartsProvider declaredPartsProvider, string requestParameterName)
    {
        foreach (var name in declaredPartsProvider.GetAllNamesDeclared())
        {
            code = code.Replace(name,
                $"GetDeclaredPart(" +
                $"\"{name}\", {requestParameterName})");
        }

        return code;
    }
    
    private static string ReplaceVariableNamesForRepeat(string code, IDeclaredPartsProvider declaredPartsProvider)
    {
        foreach (var name in declaredPartsProvider.GetAllNamesDeclared())
        {
            code = code.Replace(name, $"GetDeclaredPart(\"{name}\")");
        }

        return code;
    }

    private static string GetClassName(string prefix, string code)
    {
        return prefix + "_" + Sha1.Create(code);
    }

    public void Dispose()
    {
        var stat = _compilationStatistic;
        var nl = Constants.NewLine;
        _logger.LogInformation($"Dynamic csharp compilation statistic: " + nl +
                               $"Revision: {_revision}" + nl +
                               $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + nl +
                               $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + nl +
                               $"Count load assemblies: {stat.CountLoadAssemblies}. " + nl +
                               $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + nl);

        _unLoader.UnloadUnused(new DynamicAssembliesContext(_revision, _context));

        _logger.LogInformation("Count dynamic assemblies in current domain: " +
                               AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .Count(x => x.GetName().Name?.StartsWith(AssemblyPrefix) == true));
    }
}