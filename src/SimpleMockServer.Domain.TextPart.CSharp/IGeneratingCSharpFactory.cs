using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public interface IGeneratingCSharpFactory
{
    IObjectTextPart Create(string code, IReadOnlyCollection<Variable> variables, char variablePrefix);
}

class GeneratingCSharpFactory : IGeneratingCSharpFactory
{
    private readonly ILogger _logger;

    public GeneratingCSharpFactory(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public IObjectTextPart Create(string code, IReadOnlyCollection<Variable> variables, char variablePrefix)
    {
        var className = GetClassName(code);

        bool isGlobalTextPart = !code.Contains(CodeTemplate.ClassTemplate.ExternalRequestVariableName);
        
        code = ReplaceVariableNames(code, variablePrefix);
        
        string classTemplate = isGlobalTextPart
            ? CodeTemplate.ClassTemplate.IGlobalObjectTextPart
            : CodeTemplate.ClassTemplate.IObjectTextPart;

        string classToCompile = classTemplate
            .Replace("{code}", GetMethodBody(code))
            .Replace("{className}", className);
        
        var sw = Stopwatch.StartNew();

        var ass = DynamicClassLoader.Compile(classToCompile,
            typeof(IObjectTextPart).Assembly,
            typeof(Variable).Assembly,
            typeof(RequestData).Assembly);

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogInformation($"Compilation '{code}' took {elapsed} ms");

        var type = ass.GetTypes().Single(t => t.Name == className);
        dynamic instance = Activator.CreateInstance(type, variables)!;

        return isGlobalTextPart ? new GlobalDynamicClassWrapper(instance) : new DynamicClassWrapper(instance);
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

    private static string ReplaceVariableNames(string code, char variablePrefix)
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
                $"\"{name.TrimStart(variablePrefix)}\", {CodeTemplate.ClassTemplate.RequestParameterName})");
        }
        
        return code;
    }

    class GlobalDynamicClassWrapper : IGlobalObjectTextPart
    {
        private readonly dynamic _instance;

        public GlobalDynamicClassWrapper(dynamic instance)
        {
            _instance = instance;
        }

        public object? Get(RequestData request)
        {
            object? result = _instance.Get(request);
            return result;
        }

        public dynamic? Get()
        {
            object? result = _instance.Get();
            return result;
        }
    }
    
    class DynamicClassWrapper : IObjectTextPart
    {
        private readonly dynamic _instance;

        public DynamicClassWrapper(dynamic instance)
        {
            _instance = instance;
        }

        public object? Get(RequestData request)
        {
            object? result = _instance.Get(request);
            return result;
        }
    }

    private static string GetClassName(string code)
    {
        return "_" + HashUtils.GetSha1(code);
    }
}
