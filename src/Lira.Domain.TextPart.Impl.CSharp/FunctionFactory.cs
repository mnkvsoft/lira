using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;
using Lira.Domain.DataModel;
using Lira.Domain.Handling.Actions;
using Lira.Domain.TextPart.Impl.Custom;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

class FunctionFactory : IFunctionFactoryCSharp
{
    private readonly CompilationStatistic _compilationStatistic;
    private readonly Compiler _compiler;
    private readonly Cache _cache;
    private readonly IRangesProvider _rangesProvider;
    private readonly IImmutableList<string> _assembliesLocations;
    private readonly AssembliesLoader _assembliesLoader;
    private readonly CsFilesAssembly? _csFilesAssembly;
    private readonly Namer _namer;
    private readonly string? _globalUsingFileContent;

    public record Dependencies(
        AssembliesLoader AssembliesLoader,
        Compiler Compiler,
        CompilationStatistic CompilationStatistic,
        Cache Cache,
        IRangesProvider RangesProvider,
        Namer Namer);

    public FunctionFactory(Dependencies dependencies,
        IImmutableList<string> assembliesLocations,
        CsFilesAssembly? csFilesAssembly,
        string? globalUsingFileContent)
    {
        _compiler = dependencies.Compiler;
        _compilationStatistic = dependencies.CompilationStatistic;
        _cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
        _assembliesLoader = dependencies.AssembliesLoader;
        _namer = dependencies.Namer;
        _globalUsingFileContent = globalUsingFileContent;

        _assembliesLocations = assembliesLocations;
        _csFilesAssembly = csFilesAssembly;
    }

    public CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(
        FunctionFactoryRuleContext ruleContext, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "GeneratingFunction";
        var className = _namer.GetClassName(prefix, code);

        var classToCompile =
            CreateGeneratingFunctionClassCode(className, ruleContext, code);

        var result = CreateFunctionResult<IObjectTextPart>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "TransformFunction";
        string className = _namer.GetClassName(prefix, code);

        var (withoutUsings, usings) = ExtractUsings(code.ToString());

        string classToCompile = ClassCodeCreator.CreateTransformFunction(
            className,
            WrapToTryCatch(new Code(ForCompile: $"return {withoutUsings};", Source: code.ToString())),
            ReservedVariable.Value,
            usings,
            GetNamespaces(),
            GetUsingStatic());

        var result = CreateFunctionResult<ITransformFunction>(
            assemblyPrefixName: prefix,
            classToCompile,
            className);

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IPredicateFunction> TryCreatePredicateFunction(FunctionFactoryRuleContext ruleContext, string code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "PredicateFunction";
        string className = _namer.GetClassName(prefix, code);

        string classToCompile = ClassCodeCreator.CreatePredicateFunction(
            className,
            WrapToTryCatch(new Code(ForCompile: $"return {code};", Source: code)),
            ReservedVariable.Req,
            GetNamespaces(),
            GetUsingStatic());

        var result = CreateFunctionResult<IPredicateFunction>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IMatchFunctionTyped> TryCreateMatchFunction(
        FunctionFactoryRuleContext ruleContext, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "MatchFunction";
        string className = _namer.GetClassName(prefix, code);

        var (withoutUsings, usings) = PrepareCode(code, ruleContext);

        string classToCompile = ClassCodeCreator.CreateMatchFunction(
            className,
            GetMethodBody(new Code(ForCompile: withoutUsings, Source: code.ToString())),
            ReservedVariable.Value,
            usings,
            GetNamespaces(),
            GetUsingStatic());

        var result = CreateFunctionResult<IMatchFunctionTyped>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(
        FunctionFactoryRuleContext ruleContext, CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "RequestMatcher";
        string className = _namer.GetClassName(prefix, code);
        var (withoutUsings, usings) = PrepareCode(code, ruleContext);
        string classToCompile = ClassCodeCreator.CreateRequestMatcher(
            className,
            GetMethodBody(new Code(ForCompile: withoutUsings, Source: code.ToString())),
            ReservedVariable.Req,
            usings,
            GetNamespaces(),
            GetUsingStatic());

        var result = CreateFunctionResult<IRequestMatcher>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public CreateFunctionResult<IAction> TryCreateAction(
        FunctionFactoryRuleContext ruleContext,
        CodeBlock code)
    {
        var sw = Stopwatch.StartNew();

        string prefix = "Action";
        string className = _namer.GetClassName(prefix, code);

        string classToCompile = CreateActionClassCode(className, ruleContext, code);

        var result = CreateFunctionResult<IAction>(
            assemblyPrefixName: prefix,
            classToCompile,
            className,
            CreateDependenciesBase(ruleContext.DeclaredItemsProvider));

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    private CreateFunctionResult<TFunction> CreateFunctionResult<TFunction>(
        string assemblyPrefixName,
        string classToCompile,
        string className,
        params object[] dependencies)
    {
        var assemblyName = _namer.GetAssemblyName(assemblyPrefixName + className);

        var classesToCompiles = new List<string>(2) { classToCompile };

        if(_globalUsingFileContent != null)
            classesToCompiles.Add(_globalUsingFileContent);

        var compileResult = _compiler.Compile(
            new CompileUnit(
                AssemblyName: assemblyName,
                classesToCompiles.ToImmutableArray(),
                new References(
                    AssembliesLocations: _assembliesLocations,
                    Runtime: _csFilesAssembly != null ? [_csFilesAssembly.PeImage] : Array.Empty<PeImage>())));

        if (compileResult is CompileResult.Fault fault)
            return new CreateFunctionResult<TFunction>.Failed(fault.Message, classToCompile);

        var peImage = ((CompileResult.Success)compileResult).PeImage;

        var classAssembly = _assembliesLoader.Load(peImage);

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        var function = (TFunction)Activator.CreateInstance(type, dependencies)!;

        return new CreateFunctionResult<TFunction>.Success(function);
    }

    const string ContextParameterName = "__ctx";

    private string CreateGeneratingFunctionClassCode(
        string className,
        FunctionFactoryRuleContext ruleContext,
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
            (toCompile, usings) = PrepareCode(code, ruleContext);
        }

        string classToCompile = ClassCodeCreator.CreateIObjectTextPart(
            className,
            GetMethodBody(new Code(
                ForCompile: toCompile,
                Source: code.ToString())),
            ContextParameterName,
            ReservedVariable.Req,
            repeatFunctionName,
            usings,
            GetNamespaces(),
            GetUsingStatic());

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

    private static string ReplaceVariableNames(CodeBlock code, IDeclaredItemsProvider declaredItemsProvider)
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
                var type = declaredItemsProvider.Get(readItem.ItemName).ReturnType;

                sbCodeWithLiraItems.Append(
                    $"({(type == null || !type.NeedTyped ? "" : "(" + type.DotnetType.FullName + ")")}GetDeclaredPart(" +
                    $"\"{readItem.ItemName}\", {ContextParameterName}))");
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

    private string CreateActionClassCode(
        string className,
        FunctionFactoryRuleContext ruleContext,
        CodeBlock code)
    {
        var (withoutUsings, usings) = PrepareCode(code, ruleContext);

        string classToCompile = ClassCodeCreator.CreateAction(
            className,
            WrapToTryCatch(new Code(
                ForCompile: withoutUsings + ";",
                Source: code.ToString())),
            ContextParameterName,
            ReservedVariable.Req,
            usings,
            GetNamespaces(),
            GetUsingStatic());

        return classToCompile;
    }

    private string[] GetNamespaces()
    {
        if(_csFilesAssembly == null)
            return [];

        return _csFilesAssembly.Loaded.GetTypes()
            .Where(x => x.IsVisible && x.Namespace?.StartsWith("_") == true)
            .Select(t => t.Namespace!)
            .Distinct()
            .ToArray();
    }

    private string[] GetUsingStatic()
    {
        if(_csFilesAssembly == null)
            return [];

        return _csFilesAssembly.Loaded.GetTypes()
            .Where(x => x.IsVisible && x.Name.StartsWith("_"))
            .Select(t => t.FullName!)
            .Distinct()
            .ToArray();
    }

    private (string Code, IReadOnlyCollection<string> Usings) PrepareCode(
        CodeBlock code,
        FunctionFactoryRuleContext ruleContext)
    {
        var codeWithVariables = ReplaceVariableNames(code, ruleContext.DeclaredItemsProvider);
        var (c, usings) = ExtractUsings(codeWithVariables);
        return (c, usings.Concat(ruleContext.UsingContext.FileUsings).ToArray());
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

    private DynamicObjectBase.DependenciesBase CreateDependenciesBase(IDeclaredItemsProvider declaredItemsProvider)
    {
        return new DynamicObjectBase.DependenciesBase(_cache, _rangesProvider, declaredItemsProvider);
    }
}