using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.RuntimeCompilation;
// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

public record GeneratingCSharpContext(DynamicAssembliesRegistry Registry, CustomAssembly? CustomAssembly);
public record GeneratingCSharpVariablesContext(IReadOnlyCollection<Variable> Variables, char VariablePrefix);

public interface IGeneratingCSharpFactory
{
    IObjectTextPart Create(
        GeneratingCSharpContext context, 
        GeneratingCSharpVariablesContext variablesContext, 
        string code);
    
    ITransformFunction CreateTransform(GeneratingCSharpContext context, string code);
}

class GeneratingCSharpFactory : IGeneratingCSharpFactory
{
    private readonly ILogger _logger;

    public GeneratingCSharpFactory(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public ITransformFunction CreateTransform(GeneratingCSharpContext context, string code)
    {
        string className = GetClassName(code);
        
        var customAssembly = context.CustomAssembly;
        string classToCompile = ClassCodeCreator.CreateITransformFunction(
            className, 
            code, 
            "@value", 
            GetNamespaces(customAssembly?.Assembly),
            GetUsingStatic(customAssembly?.Assembly));

        var sw = Stopwatch.StartNew();

        var ass = context.Registry.Load(DynamicClassLoader.Compile(
            new string[] { classToCompile },
            assemblyName: "DynamicTransformFunction_" + Path.GetRandomFileName(),
            new UsageAssemblies(
                Compiled: new Assembly[]
                {
                    typeof(IObjectTextPart).Assembly,
                },
                Runtime: customAssembly == null ? Array.Empty<byte[]>() : new[] { customAssembly.PeImage })));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogDebug($"Compilation '{code}' took {elapsed} ms");

        var type = ass.GetTypes().Single(t => t.Name == className);
        return (ITransformFunction)Activator.CreateInstance(type)!;
    }

    public IObjectTextPart Create(GeneratingCSharpContext context, GeneratingCSharpVariablesContext variablesContext, string code)
    {
        var (className, classToCompile) = CreateClassCode(context, variablesContext, code);

        var sw = Stopwatch.StartNew();

        var ass = context.Registry.Load(DynamicClassLoader.Compile(
            new string[] { classToCompile },
            assemblyName: "DynamicTextPart_" + Path.GetRandomFileName(),
            new UsageAssemblies(
                Compiled: new Assembly[]
                {
                    typeof(IObjectTextPart).Assembly,
                    typeof(Variable).Assembly,
                    typeof(RequestData).Assembly,
                    Assembly.GetExecutingAssembly()
                },
                Runtime: context.CustomAssembly == null ? Array.Empty<byte[]>() : new[] { context.CustomAssembly.PeImage })));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogDebug($"Compilation '{code}' took {elapsed} ms");

        var type = ass.GetTypes().Single(t => t.Name == className);
        return (IObjectTextPart)Activator.CreateInstance(type, variablesContext.Variables)!;
    }

    private static (string className, string classToCompile) CreateClassCode(GeneratingCSharpContext context, GeneratingCSharpVariablesContext variablesContext, string code)
    {
        const string externalRequestVariableName = "@req";
        const string requestParameterName = "_request_";

        bool isGlobalTextPart = !code.Contains(externalRequestVariableName);

        code = ReplaceVariableNames(code, variablesContext.VariablePrefix, requestParameterName);

        var className = GetClassName(code);
        var customAssembly = context.CustomAssembly?.Assembly;

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
}
