using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Configuration;
using Lira.Domain.Actions;
using Lira.Domain.Matching.Request;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.TextPart.Types;
using Lira.Domain.DataModel;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactory : IFunctionFactoryCSharp
{
    private static int _revisionCounter;

    private readonly ILogger _logger;
    private readonly string _path;
    private readonly string _libsPath;

    private readonly AssemblyLoadContext _context = new(name: null, isCollectible: true);
    private readonly int _revision;

    private const string AssemblyPrefix = "__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{_revision}_{name}";

    private readonly CompilationStatistic _compilationStatistic;
    private readonly DynamicAssembliesUnloader _unLoader;
    private readonly Compiler _compiler;
    private readonly Cache _cache;
    private readonly IRangesProvider _rangesProvider;

    public FunctionFactory(IConfiguration configuration, ILoggerFactory loggerFactory,
        DynamicAssembliesUnloader unLoader,
        Compiler compiler, CompilationStatistic compilationStatistic, Cache cache, IRangesProvider rangesProvider)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();
        _libsPath = configuration.GetLibsPath() ?? _path;
        _revision = ++_revisionCounter;
        _unLoader = unLoader;
        _compiler = compiler;
        _compilationStatistic = compilationStatistic;
        _cache = cache;
        _rangesProvider = rangesProvider;
    }

    public CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(
        IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "GeneratingFunction";
        var customAssemblies = GetCustomAssemblies();
        var className = GetClassName(prefix, code);

        var classToCompile =
            CreateGeneratingFunctionClassCode(className, customAssemblies.Loaded, declaredPartsProvider, code);

        var result = CreateFunctionResult<IObjectTextPart>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies.PeImages,
            new DynamicObjectWithDeclaredPartsBase.Dependencies(
                new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider), declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(string code)
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
            classToCompile,
            className,
            customAssemblies.PeImages);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IMatchFunction> TryCreateMatchFunction(string code)
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
            classToCompile,
            className,
            customAssemblies.PeImages,
            new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "RequestMatcher";
        string className = GetClassName(prefix, code);
        var customAssemblies = GetCustomAssemblies();

        string classToCompile = ClassCodeCreator.CreateRequestMatcher(
            className,
            GetMethodBody(new Code(ForCompile: code, Source: code)),
            ReservedVariable.Req,
            GetNamespaces(customAssemblies.Loaded),
            GetUsingStatic(customAssemblies.Loaded));

        var result = CreateFunctionResult<IRequestMatcher>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies.PeImages,
            new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider));

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
            classToCompile,
            className,
            customAssemblies.PeImages,
            new DynamicObjectWithDeclaredPartsBase.Dependencies(CreateDependenciesBase(), declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    private CreateFunctionResult<TFunction> CreateFunctionResult<TFunction>(
        string assemblyPrefixName,
        string classToCompile,
        string className,
        IReadOnlyCollection<PeImage> peImages,
        params object[] dependencies)
    {
        var compileResult = _compiler.Compile(
            new CompileUnit(
                new[] { classToCompile },
                AssemblyName: GetAssemblyName(assemblyPrefixName + className),
                new UsageAssemblies(
                    AssembliesLocations: GetAssembliesLocations(),
                    Runtime: peImages)));

        if (compileResult is CompileResult.Fault fault)
            return new CreateFunctionResult<TFunction>.Failed(fault.Message, classToCompile);

        var peImage = ((CompileResult.Success)compileResult).PeImage;
        var classAssembly = Load(peImage);

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        var function = (TFunction)Activator.CreateInstance(type, dependencies)!;

        return new CreateFunctionResult<TFunction>.Success(function);
    }

    private IReadOnlyCollection<string>? _assembliesLocations = null;
    private IReadOnlyCollection<string> GetAssembliesLocations()
    {
        if (_assembliesLocations != null)
            return _assembliesLocations;

        var result = new List<string>
        {
            typeof(IObjectTextPart).Assembly.Location,
            typeof(IRangesProvider).Assembly.Location,
            typeof(Constants).Assembly.Location,
            typeof(Json).Assembly.Location,
            typeof(RequestData).Assembly.Location,
            GetType().Assembly.Location
        };

        var dllFiles = GetDllFilesLocations();
        result.AddRange(dllFiles);

        _assembliesLocations = result;
        return result;
    }

    private IReadOnlyCollection<string>? _dllLocations = null;
    private IReadOnlyCollection<string> GetDllFilesLocations()
    {
        if (_dllLocations != null)
            return _dllLocations;

        var result = new List<string>();
        result.AddRange(Directory.GetFiles(_libsPath, "*.dll", SearchOption.AllDirectories));
        _dllLocations = result;
        return result;
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

        var codes = csharpFiles.Select(File.ReadAllText).ToArray();

        var result = new List<CustomAssembly>();

        var compileResult = _compiler.Compile(
            new CompileUnit(
                codes,
                GetAssemblyName(GetClassName("CustomType", codes)),
                UsageAssemblies: new UsageAssemblies(Runtime: Array.Empty<PeImage>(), AssembliesLocations: GetDllFilesLocations())));

        var nl = Constants.NewLine;

        if (compileResult is CompileResult.Fault fault)
            throw new Exception("An error occurred while compile C# files. " + fault.Message + nl + "Files:" + nl + nl +
                                csharpFiles.JoinWithNewLine());

        var peImage = ((CompileResult.Success)compileResult).PeImage;
        var assembly = Load(peImage);
        result.Add(new CustomAssembly(assembly, peImage));

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

    const string ContextParameterName = "__ctxt";

    private static string CreateGeneratingFunctionClassCode(string className,
        IReadOnlyCollection<Assembly> customAssemblies,
        IDeclaredPartsProvider declaredPartsProvider, string code)
    {
        const string repeatFunctionName = "repeat";

        var (withoutUsings, usings) = ExtractUsings(code);

        string classToCompile = ClassCodeCreator.CreateIObjectTextPart(
            className,
            GetMethodBody(new Code(
                ForCompile:
                withoutUsings.StartsWith(repeatFunctionName)
                    ? ReplaceVariableNamesForRepeat(withoutUsings, declaredPartsProvider)
                    : ReplaceVariableNames(withoutUsings, declaredPartsProvider, ContextParameterName),
                Source: withoutUsings)),
            ContextParameterName,
            ReservedVariable.Req,
            repeatFunctionName,
            GetNamespaces(customAssemblies),
            GetUsingStatic(customAssemblies),
            usings);

        return classToCompile;
    }

    private static string CreateActionClassCode(
        string className,
        IReadOnlyCollection<Assembly> customAssemblies,
        IDeclaredPartsProvider declaredPartsProvider,
        string code)
    {
        string classToCompile = ClassCodeCreator.CreateAction(
            className,
            WrapToTryCatch(new Code(
                ForCompile: ReplaceVariableNames(code, declaredPartsProvider, ContextParameterName) + ";",
                Source: code)),
            ContextParameterName,
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

    private static (string Code, IReadOnlyCollection<string> Usings) ExtractUsings(string code)
    {
        StringBuilder newCode = new();
        var usings = new List<string>();

        foreach (var line in code.Split(Constants.NewLine))
        {
            if (line.StartsWith("@using"))
                usings.Add(line[1..]);
            else
            {
                newCode.AppendLine(line);
            }
        }
        return (newCode.ToString(), usings);
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
                    ForCompile = $"var __result = {code.ForCompile};" + Constants.NewLine +
                                 "; return __result;"
                });
        }

        return methodBody;
    }

    private static string WrapToTryCatch(Code code)
    {
        string escapedCode = code.Source
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", "\" + nl + \"")
            .Replace("\n", "\" + nl + \"");

        return @"try
            {
                " + code.ForCompile + @"
            }
            catch (Exception e)
            {
                var nl = Lira.Common.Constants.NewLine;
                throw new Exception(" + "\"An error has occurred while execute code block: \" + nl + \"" +
               escapedCode +
               "\", e);" + @"
            }";
    }

    private static string ReplaceVariableNames(string code, IDeclaredPartsProvider declaredPartsProvider,
        string requestParameterName)
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

    private static string GetClassName(string prefix, params string[] codes)
    {
        return prefix + "_" + Sha1.Create(string.Concat(codes));
    }

    public void Dispose()
    {
        var stat = _compilationStatistic;
        var nl = Constants.NewLine;
        _logger.LogDebug($"Dynamic csharp compilation statistic: " + nl +
                         $"Revision: {_revision}" + nl +
                         $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + nl +
                         $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + nl +
                         $"Count load assemblies: {stat.CountLoadAssemblies}. " + nl +
                         $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + nl);

        _unLoader.UnloadUnused(new DynamicAssembliesContext(_revision, _context));

        _logger.LogDebug("Count dynamic assemblies in current domain: " +
                         AppDomain.CurrentDomain
                             .GetAssemblies()
                             .Count(x => x.GetName().Name?.StartsWith(AssemblyPrefix) == true));
    }

    private DynamicObjectBase.DependenciesBase CreateDependenciesBase()
    {
        return new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider);
    }
}