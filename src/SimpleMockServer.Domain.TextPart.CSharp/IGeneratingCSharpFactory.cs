using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Configuration;
using SimpleMockServer.Domain.TextPart.CSharp.Compilation;
using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
using SimpleMockServer.Domain.TextPart.Variables;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

public record GeneratingCSharpVariablesContext(IReadOnlyCollection<Variable> Variables, char VariablePrefix);

public interface IGeneratingCSharpFactory : IDisposable
{
    IObjectTextPart Create(
        GeneratingCSharpVariablesContext variablesContext,
        string code);

    ITransformFunction CreateTransform(string code);
}

class GeneratingCSharpFactory : IGeneratingCSharpFactory
{
    private static int RevisionCounter;

    private readonly ILogger _logger;
    private readonly string _path;

    private readonly AssemblyLoadContext _context = new(null);
    private readonly int _revision;

    private const string AssemblyPrefix = "__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{_revision}_{name}";

    private readonly CompilationStatistic _compilationStatistic;
    private readonly DynamicAssembliesUploader _unLoader;
    private readonly Compiler _compiler;

    public GeneratingCSharpFactory(IConfiguration configuration, ILoggerFactory loggerFactory, DynamicAssembliesUploader unLoader, Compiler compiler, CompilationStatistic compilationStatistic)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();
        _revision = ++RevisionCounter;
        _unLoader = unLoader;
        _compiler = compiler;
        _compilationStatistic = compilationStatistic;
    }

    public ITransformFunction CreateTransform(string code)
    {
        var sw = Stopwatch.StartNew();

        string className = GetClassName(code);
        var customAssemblies = GetCustomAssemblies();

        string classToCompile = ClassCodeCreator.CreateITransformFunction(
            className,
            code,
            "@value",
            GetNamespaces(customAssemblies.Loaded),
            GetUsingStatic(customAssemblies.Loaded));

        var ass = Load(_compiler.Compile(
            new CompileUnit(
                classToCompile,
                AssemblyName: GetAssemblyName("TransformFunction" + className),
                new UsageAssemblies(
                    Compiled: new Assembly[] { typeof(IObjectTextPart).Assembly, },
                    Runtime: customAssemblies.PeImages))));

        var type = ass.GetTypes().Single(t => t.Name == className);

        var result = (ITransformFunction)Activator.CreateInstance(type)!;

        _compilationStatistic.AddTotalTime(sw.Elapsed);

        return result;
    }

    public IObjectTextPart Create(GeneratingCSharpVariablesContext variablesContext, string code)
    {
        var sw = Stopwatch.StartNew();

        var customAssemblies = GetCustomAssemblies();

        var (className, classToCompile) = CreateClassCode(customAssemblies.Loaded, variablesContext, code);

        var classAssembly = Load(_compiler.Compile(
            new CompileUnit(
                classToCompile,
                AssemblyName: GetAssemblyName("ObjectTextPart" + className),
                new UsageAssemblies(
                    Compiled: new Assembly[]
                    {
                        typeof(IObjectTextPart).Assembly, 
                        typeof(Variable).Assembly, 
                        typeof(RequestData).Assembly,
                        GetType().Assembly
                    },
                    Runtime: customAssemblies.PeImages))));

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        var result = (IObjectTextPart)Activator.CreateInstance(type, variablesContext.Variables)!;

        _compilationStatistic.AddTotalTime(sw.Elapsed);

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

        var codes = csharpFiles.Select(File.ReadAllText).ToList();

        var result = new List<CustomAssembly>();

        foreach (var code in codes)
        {
            var peImage = _compiler.Compile(new CompileUnit(code, GetAssemblyName("CustomType_" + GetClassName(code)), UsageAssemblies: null));
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

    private static (string className, string classToCompile) CreateClassCode(IReadOnlyCollection<Assembly> customAssemblies,
        GeneratingCSharpVariablesContext variablesContext, string code)
    {
        var sw = Stopwatch.StartNew();

        const string externalRequestVariableName = "@req";
        const string requestParameterName = "_request_";

        bool isGlobalTextPart = !code.Contains(externalRequestVariableName);

        code = ReplaceVariableNames(code, variablesContext.VariablePrefix, requestParameterName);

        var className = GetClassName(code);

        string classToCompile = isGlobalTextPart
            ? ClassCodeCreator.CreateIGlobalObjectTextPart(
                className,
                GetMethodBody(code),
                requestParameterName,
                GetNamespaces(customAssemblies),
                GetUsingStatic(customAssemblies))
            : ClassCodeCreator.CreateIObjectTextPart(
                className,
                GetMethodBody(code),
                requestParameterName,
                externalRequestVariableName,
                GetNamespaces(customAssemblies),
                GetUsingStatic(customAssemblies));

        return (className, classToCompile);
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

    private static string GetMethodBody(string code)
    {
        string methodBody;

        var nl = Environment.NewLine;
        if (code.Contains("return"))
        {
            methodBody = code;
        }
        else
        {
            methodBody = $"var _result_ = {code};" + nl +
                         "return _result_;";
        }

        return methodBody;
    }

    private static string ReplaceVariableNames(string code, char variablePrefix, string requestParameterName)
    {
        using var enumerator = code.GetEnumerator();

        bool isString = false;
        var variablesToReplace = new List<string>();

        var curVariable = new StringBuilder();
        while (enumerator.MoveNext())
        {
            char c = enumerator.Current;

            if (isString)
            {
                if (c != '"')
                    continue;

                if (c == '"')
                {
                    isString = false;
                    continue;
                }
            }

            if (c == '"')
            {
                isString = true;
                continue;
            }

            if (curVariable.Length != 0)
            {
                if (Variable.IsAllowedCharInName(c))
                {
                    curVariable.Append(c);
                    continue;
                }
                else
                {
                    string varName = curVariable.ToString();

                    if (!variablesToReplace.Contains(varName))
                        variablesToReplace.Add(varName);

                    curVariable.Clear();
                }
            }

            if (c == '$')
                curVariable.Append(c);
        }

        foreach (var name in variablesToReplace)
        {
            code = code.Replace(name,
                $"GetVariable(" +
                $"\"{name.TrimStart(variablePrefix)}\", {requestParameterName})");
        }

        return code;
    }

    private static string GetClassName(string code)
    {
        return "_" + Sha1.Create(code);
    }

    public void Dispose()
    {
        var stat = _compilationStatistic;
        var nl = Environment.NewLine;
        _logger.LogInformation($"Dynamic csharp compilation statistic: " + nl +
                               $"Revision: {_revision}" + nl +
                               $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + nl +
                               $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + nl +
                               $"Count load assemblies: {stat.CountLoadAssemblies}. " + nl +
                               $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + nl);

        _unLoader.UnloadUnused(new DynamicAssembliesContext(_revision, _context));

        _logger.LogInformation($"Count dynamic assemblies in current domain: " +
                               AppDomain.CurrentDomain.GetAssemblies()
                                   .Where(x => x.GetName().Name?.StartsWith(AssemblyPrefix) == true)
                                   .Count());
    }
}
