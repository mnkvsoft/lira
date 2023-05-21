using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.RuntimeCompilation;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public interface IGeneratingCSharpFactory
{
    IObjectTextPart Create(string code, CompileResult? customAssembly, IReadOnlyCollection<Variable> variables, char variablePrefix);
    ITransformFunction CreateTransform(string code, CompileResult? customAssembly);
}

class GeneratingCSharpFactory : IGeneratingCSharpFactory
{
    private readonly ILogger _logger;

    public GeneratingCSharpFactory(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public ITransformFunction CreateTransform(string code, CompileResult? customAssembly)
    {
        var className = GetClassName(code);
        string classToCompile = ClassCodeCreator.CreateITransformFunction(className, code, "@value", GetNamespaces(customAssembly?.Assembly));

        var sw = Stopwatch.StartNew();

        var ass = DynamicClassLoader.Compile(
            new string[] { classToCompile },
            assemblyName: "DynamicTransformFunction_" + Path.GetRandomFileName(),
            new UsageAssemblies(
                Compiled: new Assembly[]
                {
                    typeof(IObjectTextPart).Assembly,
                },
                Runtime: customAssembly == null ? Array.Empty<byte[]>() : new[] { customAssembly.Bytes }));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogInformation($"Compilation '{code}' took {elapsed} ms");

        var type = ass.Assembly.GetTypes().Single(t => t.Name == className);
        return (ITransformFunction)Activator.CreateInstance(type)!;
    }

    public IObjectTextPart Create(string code, CompileResult? customAssembly, IReadOnlyCollection<Variable> variables, char variablePrefix)
    {
        var customNamespaces = GetNamespaces(customAssembly?.Assembly);

        const string externalRequestVariableName = "@req";
        const string requestParameterName = "_request_";

        bool isGlobalTextPart = !code.Contains(externalRequestVariableName);

        code = ReplaceVariableNames(code, variablePrefix, requestParameterName);

        var className = GetClassName(code);
        string classToCompile =
            isGlobalTextPart
            ? ClassCodeCreator.CreateIGlobalObjectTextPart(className, GetMethodBody(code), requestParameterName, customNamespaces)
            : ClassCodeCreator.CreateIObjectTextPart(className, GetMethodBody(code), requestParameterName, externalRequestVariableName, customNamespaces);

        var sw = Stopwatch.StartNew();

        var ass = DynamicClassLoader.Compile(
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
                Runtime: customAssembly == null ? Array.Empty<byte[]>() : new[] { customAssembly.Bytes }));

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogInformation($"Compilation '{code}' took {elapsed} ms");

        var type = ass.Assembly.GetTypes().Single(t => t.Name == className);
        return (IObjectTextPart)Activator.CreateInstance(type, variables)!;
    }

    private static string[] GetNamespaces(Assembly? customAssembly)
    {
        return customAssembly != null
                    ? customAssembly.GetTypes()
                        .Where(x => x.IsVisible && x.Namespace != null)
                        .Select(t => t.Namespace!)
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

    class DynamicTransformFunctionWrapper : ITransformFunction
    {
        private readonly dynamic _instance;

        public DynamicTransformFunctionWrapper(dynamic instance)
        {
            _instance = instance;
        }

        public dynamic Transform(dynamic? dynamic)
        {
            return _instance.Transform(dynamic);
        }
    }

    private static string GetClassName(string code)
    {
        return "_" + HashUtils.GetSha1(code);
    }
}
