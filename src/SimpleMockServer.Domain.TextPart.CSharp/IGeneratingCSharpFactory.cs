using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public interface IGeneratingCSharpFactory
{
    IObjectTextPart Create(string code);
}

class GeneratingCSharpFactory : IGeneratingCSharpFactory
{
    private readonly ILogger _logger;

    public GeneratingCSharpFactory(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public IObjectTextPart Create(string code)
    {
        var className = GetClassName(code);

        string classTemplate = CodeTemplate.ClassTemplate.IObjectTextPart
            .Replace("{code}", code)
            .Replace("{className}", className);

        var sw = Stopwatch.StartNew();
        
        var ass = DynamicClassLoader.Compile(classTemplate, 
            typeof(IObjectTextPart).Assembly,
            typeof(RequestData).Assembly);

        var elapsed = sw.ElapsedMilliseconds;

        _logger.LogInformation($"Compilation '{code}' took {elapsed} ms");
        
        var type = ass.GetTypes().Single(t => t.Name == className);
        dynamic instance = Activator.CreateInstance(type)!;

        return new DynamicClassWrapper(instance);
        
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
