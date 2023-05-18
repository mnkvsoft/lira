using System.Diagnostics;
using System.Text;
using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.CSharp;

class CompilationTextPartCreator
{
    public static IObjectTextPart Create(string code)
    {
        var className = GetClassName(code);

        var sbClass = new StringBuilder();
        
        sbClass.Append(CodeTemplate.ImportNamespaces);
        sbClass.Append(CodeTemplate.Namespace);
        sbClass.Append(CodeTemplate.ClassTemplate.IObjectTextPart);
        
        string classTemplate = sbClass.ToString()
            .Replace("{code}", code)
            .Replace("{className}", className);

        var sw = Stopwatch.StartNew();
        
        var ass = DynamicClassLoader.Compile(classTemplate, 
            typeof(IObjectTextPart).Assembly,
            typeof(RequestData).Assembly);

        var elapsed = sw.ElapsedMilliseconds;

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
