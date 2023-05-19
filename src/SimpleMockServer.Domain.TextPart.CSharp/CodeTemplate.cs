namespace SimpleMockServer.Domain.TextPart.CSharp;

static class CodeTemplate
{
    private static readonly string Nl = Environment.NewLine;
    
    public readonly static string ImportNamespaces =
        "using System;" + Nl +
        "using System.Text;" + Nl +
        "using System.Collections;" + Nl + 
        "using System.Collections.Generic;" + Nl + Nl +

        "using SimpleMockServer.Domain;" + Nl +
        
        "using SimpleMockServer.Domain.TextPart;" + Nl +
        "using SimpleMockServer.Domain.TextPart.Variables;";

    public const string Namespace = "namespace __DynamicGenerated;";
    
    public static class ClassTemplate
    {
        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class {className} : IObjectTextPart
{
    private readonly IReadOnlyCollection<Variable> _variables;

    public {className}(IReadOnlyCollection<Variable> variables)
    {
        _variables = variables;
    }

    public object Get(RequestData request)
    {
        return {code};
    }

    private dynamic? GetVariable(string name, RequestData request)
    {
        return _variables.GetOrThrow(name).Get(request);
    }
}";
    }
}

