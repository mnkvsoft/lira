using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Configuration;
using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
using SimpleMockServer.Domain.TextPart.CSharp.RuntimeCompilation;
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

class GeneratingCSharpFactory : IGeneratingCSharpFactory, IDisposable
{
    private static int RevisionCounter;
    
    private readonly ILogger _logger;
    private readonly string _path;
    
    private readonly AssemblyLoadContext _context = new(null);
    private readonly int _revision;

    private const string AssemblyPrefix = $"__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{_revision}_{name}";

    public CompilationStatistic CompilationStatistic { get; }
    private readonly DynamicAssembliesUploader _unloader;

    public GeneratingCSharpFactory(IConfiguration configuration, ILoggerFactory loggerFactory, DynamicAssembliesUploader unloader)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();
        _revision = ++RevisionCounter;
        CompilationStatistic = new CompilationStatistic(_revision);
        _unloader = unloader;
    }

    public ITransformFunction CreateTransform(string code)
    {
        var sw = Stopwatch.StartNew();
        
        string className = GetClassName(code);
        var customAssembly = GetCustomAssembly();
        
        string classToCompile = ClassCodeCreator.CreateITransformFunction(
            className, 
            code, 
            "@value", 
            GetNamespaces(customAssembly?.LoadedAssembly),
            GetUsingStatic(customAssembly?.LoadedAssembly));

        var ass = Load(Compile(
            new string[] { classToCompile },
            assemblyName: GetAssemblyName("TransformFunction" + className),
            new UsageAssemblies(
                Compiled: new Assembly[]
                {
                    typeof(IObjectTextPart).Assembly,
                },
                Runtime: customAssembly == null ? Array.Empty<byte[]>() : new[] { customAssembly.PeImage })));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogDebug($"Compilation '{code}' took {elapsed} ms");

        var type = ass.GetTypes().Single(t => t.Name == className);
        
        var result = (ITransformFunction)Activator.CreateInstance(type)!;
        CompilationStatistic.AddTotalTime(sw.Elapsed);
        
        return result;
    }
    
    public IObjectTextPart Create(GeneratingCSharpVariablesContext variablesContext, string code)
    {
        var sw = Stopwatch.StartNew();
        var customAssembly = GetCustomAssembly();
        
        var (className, classToCompile) = CreateClassCode(customAssembly?.LoadedAssembly, variablesContext, code);

        var classAssembly = Load(Compile(
            new string[] { classToCompile },
            assemblyName: GetAssemblyName("ObjectTextPart" + className),
            new UsageAssemblies(
                Compiled: new Assembly[]
                {
                    typeof(IObjectTextPart).Assembly,
                    typeof(Variable).Assembly,
                    typeof(RequestData).Assembly,
                    Assembly.GetExecutingAssembly()
                },
                Runtime: customAssembly == null ? Array.Empty<byte[]>() : new[] { customAssembly.PeImage })));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogDebug($"Compilation '{code}' took {elapsed} ms");

        var type = classAssembly.GetTypes().Single(t => t.Name == className);
        
        var result = (IObjectTextPart)Activator.CreateInstance(type, variablesContext.Variables)!;
        CompilationStatistic.AddTotalTime(sw.Elapsed);
        
        return result;
    }
    
    private bool _wasInit;
    private CustomAssembly? _customAssembly;
    private CustomAssembly? GetCustomAssembly()
    {
        if (_wasInit)
            return _customAssembly;
            
        _customAssembly = GetCustomAssembly(_path);
        _wasInit = true;
        return _customAssembly;
    }
    
    private CustomAssembly? GetCustomAssembly(string path)
    {
        var csharpFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        if(csharpFiles.Length == 0)
            return null;

        var codes = csharpFiles.Select(File.ReadAllText).ToList();

        var compileResult = Compile(codes, GetAssemblyName("CustomTypes"));

        var assembly = Load(compileResult);
        return new CustomAssembly(assembly, compileResult.PeImage);
    }

    private Assembly Load(CompileResult compileResult)
    {
        var sw = Stopwatch.StartNew();

        using var stream = new MemoryStream();
        stream.Write(compileResult.PeImage);
        stream.Position = 0;
        var result = _context.LoadFromStream(stream);
        
        CompilationStatistic.AddLoadAssemblyTime(sw.Elapsed);
        
        return result;
    }

    CompileResult Compile(IReadOnlyCollection<string> codes, string assemblyName, UsageAssemblies? usageAssemblies = null)
    {
        var sw = Stopwatch.StartNew();
        var result = DynamicClassLoader.Compile(codes, assemblyName, usageAssemblies);
        CompilationStatistic.AddCompilationTime(sw.Elapsed);
        return result;
    }
    
    private static (string className, string classToCompile) CreateClassCode(Assembly? customAssembly, GeneratingCSharpVariablesContext variablesContext, string code)
    {
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
                GetNamespaces(customAssembly),
                GetUsingStatic(customAssembly))
            : ClassCodeCreator.CreateIObjectTextPart(
                className,
                GetMethodBody(code),
                requestParameterName,
                externalRequestVariableName,
                GetNamespaces(customAssembly),
                GetUsingStatic(customAssembly));
        
        return (className, classToCompile);
    }

    private static string[] GetNamespaces(Assembly? customAssembly)
    {
        return customAssembly != null
                    ? customAssembly.GetTypes()
                        .Where(x => x.IsVisible && x.Namespace?.StartsWith("_") == true)
                        .Select(t => t.Namespace!)
                        .Distinct()
                        .ToArray()
                    : Array.Empty<string>();
    }

    private static string[] GetUsingStatic(Assembly? customAssembly)
    {
        return customAssembly != null
                    ? customAssembly.GetTypes()
                        .Where(x => x.IsVisible && x.Name.StartsWith("_"))
                        .Select(t => t.FullName!)
                        .Distinct()
                        .ToArray()
                    : Array.Empty<string>();
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
                    
                    if(!variablesToReplace.Contains(varName))
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
        return "_" + HashUtils.GetSha1(code);
    }

    public void Dispose()
    {
        var stat = CompilationStatistic;
        _logger.LogInformation($"Dynamic csharp compilation statistic: " + Environment.NewLine +
                               $"Revision: {_revision}" + Environment.NewLine +
                               $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Count load assemblies: {stat.CountLoadAssemblies}. " + Environment.NewLine +
                               $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Max compilation time: {(int)stat.MaxCompilationTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Average compilation time: {(int)(stat.TotalCompilationTime.TotalMilliseconds / stat.CountLoadAssemblies)} ms.");

        _unloader.UnloadUnused(new DynamicAssembliesContext(_revision, _context));

        _logger.LogInformation($"Count dynamic assemblies in current domain: " +
            AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().Name?.StartsWith(AssemblyPrefix) == true)
            .Count());
    }
}
