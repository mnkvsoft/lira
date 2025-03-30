using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Configuration;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.DataModel;
using Lira.Domain.Handling.Actions;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactory : IFunctionFactoryCSharp
{
    public class State
    {
        // cannot be stored in a static variable,
        // because in tests several FunctionFactory are created in parallel
        // and because of this caching does not work correctly
        public int RevisionCounter { get; set; }
    }

    private readonly ILogger _logger;
    private readonly string _path;

    private readonly AssemblyLoadContext _context = new(name: null, isCollectible: true);
    private readonly int _revision;
    private readonly Dictionary<Hash, Assembly> _loadedAssembliesHashes = new();

    private const string AssemblyPrefix = "__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{_revision}_{name}";

    private readonly CompilationStatistic _compilationStatistic;
    private readonly DynamicAssembliesUnloader _unLoader;
    private readonly Compiler _compiler;
    private readonly Cache _cache;
    private readonly IRangesProvider _rangesProvider;
    private readonly IImmutableList<string> _assembliesLocations;

    public record Dependencies(
        IConfiguration Configuration,
        ILoggerFactory LoggerFactory,
        DynamicAssembliesUnloader UnLoader,
        Compiler Compiler,
        CompilationStatistic CompilationStatistic,
        Cache Cache,
        IRangesProvider RangesProvider,
        State State);

    public FunctionFactory(Dependencies dependencies, IImmutableList<string> assembliesLocations)
    {
        _logger = dependencies.LoggerFactory.CreateLogger(GetType());
        _path = dependencies.Configuration.GetRulesPath();

        _revision = ++dependencies.State.RevisionCounter;
        _unLoader = dependencies.UnLoader;
        _compiler = dependencies.Compiler;
        _compilationStatistic = dependencies.CompilationStatistic;
        _cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
        _assembliesLocations = assembliesLocations;
    }

    public async Task<CreateFunctionResult<IObjectTextPart>> TryCreateGeneratingFunction(
        IDeclaredPartsProvider declaredPartsProvider, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "GeneratingFunction";
        var csFilesAssembly = await GetCsFilesAssembly();
        var className = GetClassName(prefix, code);

        var classToCompile =
            CreateGeneratingFunctionClassCode(className, csFilesAssembly?.Loaded, declaredPartsProvider, code);

        var result = CreateFunctionResult<IObjectTextPart>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            csFilesAssembly?.PeImage,
            CreateDependenciesBase(declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public async Task<CreateFunctionResult<ITransformFunction>> TryCreateTransformFunction(CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "TransformFunction";
        string className = GetClassName(prefix, code);
        var customAssemblies = await GetCsFilesAssembly();

        var (withoutUsings, usings) = ExtractUsings(code.ToString());

        string classToCompile = ClassCodeCreator.CreateTransformFunction(
            className,
            WrapToTryCatch(new Code(ForCompile: $"return {withoutUsings};", Source: code)),
            ReservedVariable.Value,
            usings,
            GetNamespaces(customAssemblies?.Loaded),
            GetUsingStatic(customAssemblies?.Loaded));

        var result = CreateFunctionResult<ITransformFunction>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies?.PeImage);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public async Task<CreateFunctionResult<IMatchFunctionTyped>> TryCreateMatchFunction(
        IDeclaredPartsProvider declaredPartsProvider, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "MatchFunction";
        string className = GetClassName(prefix, code);
        var customAssemblies = await GetCsFilesAssembly();

        var (withoutUsings, usings) = ExtractUsingsAndReplaceVars(code, declaredPartsProvider);

        string classToCompile = ClassCodeCreator.CreateMatchFunction(
            className,
            GetMethodBody(new Code(ForCompile: withoutUsings, Source: code)),
            ReservedVariable.Value,
            usings,
            GetNamespaces(customAssemblies?.Loaded),
            GetUsingStatic(customAssemblies?.Loaded));

        var result = CreateFunctionResult<IMatchFunctionTyped>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies?.PeImage,
            CreateDependenciesBase(declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public async Task<CreateFunctionResult<IRequestMatcher>> TryCreateRequestMatcher(
        IDeclaredPartsProvider declaredPartsProvider, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "RequestMatcher";
        string className = GetClassName(prefix, code);
        var customAssemblies = await GetCsFilesAssembly();
        var (withoutUsings, usings) = ExtractUsingsAndReplaceVars(code, declaredPartsProvider);
        string classToCompile = ClassCodeCreator.CreateRequestMatcher(
            className,
            GetMethodBody(new Code(ForCompile: withoutUsings, Source: code)),
            ReservedVariable.Req,
            usings,
            GetNamespaces(customAssemblies?.Loaded),
            GetUsingStatic(customAssemblies?.Loaded));

        var result = CreateFunctionResult<IRequestMatcher>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies?.PeImage,
            CreateDependenciesBase(declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public async Task<CreateFunctionResult<IAction>> TryCreateAction(IDeclaredPartsProvider declaredPartsProvider,
        CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "Action";
        string className = GetClassName(prefix, code);
        var customAssemblies = await GetCsFilesAssembly();

        string classToCompile = CreateActionClassCode(className, customAssemblies?.Loaded, declaredPartsProvider, code);

        var result = CreateFunctionResult<IAction>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            customAssemblies?.PeImage,
            CreateDependenciesBase(declaredPartsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    private CreateFunctionResult<TFunction> CreateFunctionResult<TFunction>(
        string assemblyPrefixName,
        string classToCompile,
        string className,
        PeImage? csFilesPeImage,
        params object[] dependencies)
    {
        var assemblyName = GetAssemblyName(assemblyPrefixName + className);

        var compileResult = _compiler.Compile(
            new CompileUnit(
                AssemblyName: assemblyName,
                [classToCompile],
                new References(
                    AssembliesLocations: _assembliesLocations,
                    Runtime: csFilesPeImage != null ? [csFilesPeImage] : Array.Empty<PeImage>())));

        if (compileResult is CompileResult.Fault fault)
            return new CreateFunctionResult<TFunction>.Failed(fault.Message, classToCompile);

        var peImage = ((CompileResult.Success)compileResult).PeImage;

        var classAssembly = Load(peImage);

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        var function = (TFunction)Activator.CreateInstance(type, dependencies)!;

        return new CreateFunctionResult<TFunction>.Success(function);
    }

    private bool _customAssembliesWasInit;
    private CsFilesAssembly? _csFilesAssembly;

    private async Task<CsFilesAssembly?> GetCsFilesAssembly()
    {
        if (_customAssembliesWasInit)
            return _csFilesAssembly;

        _csFilesAssembly = await GetCsFilesAssembly(_path);
        _customAssembliesWasInit = true;

        return _csFilesAssembly;
    }

    private async Task<CsFilesAssembly?> GetCsFilesAssembly(string path)
    {
        var csharpFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        if (csharpFiles.Length == 0)
            return null;

        var codes = await Task.WhenAll(csharpFiles.Select(async x => await File.ReadAllTextAsync(x)));

        var compileResult = _compiler.Compile(
            new CompileUnit(
                GetAssemblyName(GetClassName("CustomType", codes)),
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

        var assembly = Load(peImage);

        return new CsFilesAssembly(assembly, peImage);
    }

    private Assembly Load(PeImage peImage)
    {
        var sw = Stopwatch.StartNew();

        using var stream = new MemoryStream();
        stream.Write(peImage.Bytes.Value);
        stream.Position = 0;

        var hash = peImage.Hash;
        if (_loadedAssembliesHashes.TryGetValue(hash, out var assembly))
            return assembly;

        var result = _context.LoadFromStream(stream);

        _compilationStatistic.AddLoadAssemblyTime(sw.Elapsed);
        _loadedAssembliesHashes.Add(hash, result);

        return result;
    }

    const string ContextParameterName = "__ctx";

    private static string CreateGeneratingFunctionClassCode(
        string className,
        Assembly? csFilesAssembly,
        IDeclaredPartsProvider declaredPartsProvider,
        CodeBlock code)
    {
        const string repeatFunctionName = "repeat";

        var sourceCode = code.ToString();
        string toCompile;
        IReadOnlyCollection<string> usings;
        if (sourceCode.StartsWith(repeatFunctionName))
        {
            toCompile = ReplaceVariableNamesForRepeat(code)
                .Replace(repeatFunctionName, "await " + repeatFunctionName);
            usings = Array.Empty<string>();
        }
        else
        {
            (toCompile, usings) = ExtractUsingsAndReplaceVars(code, declaredPartsProvider);
        }

        string classToCompile = ClassCodeCreator.CreateIObjectTextPart(
            className,
            GetMethodBody(new Code(
                ForCompile: toCompile,
                Source: code)),
            ContextParameterName,
            ReservedVariable.Req,
            repeatFunctionName,
            usings,
            GetNamespaces(csFilesAssembly),
            GetUsingStatic(csFilesAssembly));

        return classToCompile;
    }

    private static string ReplaceVariableNamesForRepeat(CodeBlock code)
    {
        var sbCodeWithLiraItems = new StringBuilder();

        foreach (var token in code.Tokens)
        {
            if (token is CodeToken.OtherCode otherCode)
            {
                sbCodeWithLiraItems.Append(otherCode.Code);
            }
            else if (token is CodeToken.ReadItem readItem)
            {
                sbCodeWithLiraItems.Append($"GetDeclaredPart(\"{readItem.ItemName}\")");
            }
            else
            {
                throw new Exception($"Unexpected token type: {token.GetType()}");
            }
        }

        return sbCodeWithLiraItems.ToString();
    }

    private static string ReplaceVariableNames(CodeBlock code, IDeclaredPartsProvider declaredPartsProvider)
    {
        var sbCodeWithLiraItems = new StringBuilder();

        foreach (var token in code.Tokens)
        {
            if (token is CodeToken.OtherCode otherCode)
            {
                sbCodeWithLiraItems.Append(otherCode.Code);
            }
            else if (token is CodeToken.ReadItem readItem)
            {
                var type = declaredPartsProvider.GetPartType(readItem.ItemName);

                sbCodeWithLiraItems.Append(
                    $"({(type == null || !type.NeedTyped ? "" : "(" + type.DotnetType.FullName + ")")}(await GetDeclaredPart(" +
                    $"\"{readItem.ItemName}\", {ContextParameterName})))");
            }
            else if (token is CodeToken.WriteItem writeItem)
            {
                sbCodeWithLiraItems.Append($"""__variablesWriter["{writeItem.ItemName}"]""");
            }
            else
            {
                throw new Exception($"Unexpected token type: {token.GetType()}");
            }
        }

        return sbCodeWithLiraItems.ToString();
    }

    private static string CreateActionClassCode(
        string className,
        Assembly? csFilesAssembly,
        IDeclaredPartsProvider declaredPartsProvider,
        CodeBlock code)
    {
        var (withoutUsings, usings) = ExtractUsingsAndReplaceVars(code, declaredPartsProvider);

        string classToCompile = ClassCodeCreator.CreateAction(
            className,
            WrapToTryCatch(new Code(
                ForCompile: withoutUsings + ";",
                Source: code)),
            ContextParameterName,
            ReservedVariable.Req,
            usings,
            GetNamespaces(csFilesAssembly),
            GetUsingStatic(csFilesAssembly));

        return classToCompile;
    }

    private static string[] GetNamespaces(Assembly? csFilesAssembly)
    {
        if(csFilesAssembly == null)
            return [];

        return csFilesAssembly.GetTypes()
            .Where(x => x.IsVisible && x.Namespace?.StartsWith("_") == true)
            .Select(t => t.Namespace!)
            .Distinct()
            .ToArray();
    }

    private static string[] GetUsingStatic(Assembly? csFilesAssembly)
    {
        if(csFilesAssembly == null)
            return [];

        return csFilesAssembly.GetTypes()
            .Where(x => x.IsVisible && x.Name.StartsWith("_"))
            .Select(t => t.FullName!)
            .Distinct()
            .ToArray();
    }

    private static (string Code, IReadOnlyCollection<string> Usings) ExtractUsingsAndReplaceVars(CodeBlock code,
        IDeclaredPartsProvider declaredPartsProvider)
    {
        var codeWithVariables = ReplaceVariableNames(code, declaredPartsProvider);
        return ExtractUsings(codeWithVariables);
    }

    private static (string Code, IReadOnlyCollection<string> Usings) ExtractUsings(string codeWithVariables)
    {
        var newCode = new StringBuilder();
        var usings = new List<string>();

        foreach (var line in codeWithVariables.Split(Constants.NewLine))
        {
            var trimmed = line.TrimStart(" ").TrimStart("\t");
            if (trimmed.StartsWith("@using"))
            {
                usings.Add(trimmed[1..]);
            }
            else
            {
                newCode.Append((newCode.Length > 0 ? Constants.NewLine : "") + line);
            }
        }

        return (newCode.ToString(), usings);
    }

    record Code(string ForCompile, CodeBlock Source);

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
        string escapedCode = code.Source.ToString()
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

    private static string GetClassName(string prefix, CodeBlock code) => GetClassName(prefix, code.ToString());

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
                         $"Count load assemblies: {stat.CountLoadAssemblies}." + nl +
                         $"Count functions: {stat.CountFunctionsTotal} (compile: {stat.CountFunctionsCompiled}, cache: {stat.CountFunctionsTotalFromCache})." + nl +
                         $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + nl);

        _unLoader.UnloadUnused(new DynamicAssembliesContext(_revision, _context));

        _logger.LogDebug("Count dynamic assemblies in current domain: " +
                         AppDomain.CurrentDomain
                             .GetAssemblies()
                             .Count(x => x.GetName().Name?.StartsWith(AssemblyPrefix) == true));
    }

    private DynamicObjectBase.DependenciesBase CreateDependenciesBase(IDeclaredPartsProvider declaredPartsProvider)
    {
        return new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider, declaredPartsProvider);
    }
}