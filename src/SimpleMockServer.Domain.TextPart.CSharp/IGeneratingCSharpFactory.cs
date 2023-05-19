using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;
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

        try
        {
            code = ReplaceVariableNames(code, variablePrefix);
        }
        catch (Exception e)
        {
            
            throw new Exception($"Code: {code}", e );
        }
        
        
        string classTemplate = CodeTemplate.ClassTemplate.IObjectTextPart
            .Replace("{code}", code)
            .Replace("{className}", className);

        var sw = Stopwatch.StartNew();
        
        var ass = DynamicClassLoader.Compile(classTemplate, 
            typeof(IObjectTextPart).Assembly,
            typeof(Variable).Assembly,
            typeof(RequestData).Assembly);

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogInformation($"Compilation '{code}' took {elapsed} ms");
        
        var type = ass.GetTypes().Single(t => t.Name == className);
        dynamic instance = Activator.CreateInstance(type, variables)!;

        return new DynamicClassWrapper(instance);
        
    }

    private static string ReplaceVariableNames(string code, char variablePrefix)
    {
        int indexOf = code.IndexOf(variablePrefix, StringComparison.Ordinal);

        while (indexOf != -1)
        {
            var sbName = new StringBuilder();
            for (int i = indexOf; i < code.Length; i++)
            {
                char c = code[i];
                if (c != variablePrefix && !Variable.IsAllowedCharInName(c))
                    break;
 
                sbName.Append(c);
            }

            string name = sbName.ToString();
            code = code.Replace(name, $"GetVariable(\"{name.TrimStart(variablePrefix)}\", request)");
            indexOf = code.IndexOf(variablePrefix, StringComparison.Ordinal);
        }

        return code;
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
