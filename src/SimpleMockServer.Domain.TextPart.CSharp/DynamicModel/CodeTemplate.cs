namespace SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;

static class CodeTemplate
{
    private static readonly string Nl = Environment.NewLine;
    
    public readonly static string ImportNamespaces =
        "using System;" + Nl +
        "using System.Text;" + Nl +
        "using System.Collections;" + Nl + 
        "using System.Collections.Generic;" + Nl + Nl +
        
        "using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;" + Nl +
        "using SimpleMockServer.Domain;" + Nl +
        
        "using SimpleMockServer.Domain.TextPart;" + Nl +
        "using SimpleMockServer.Domain.TextPart.Variables;";

    public const string Namespace = "namespace __DynamicGenerated;";
    
    public static class ClassTemplate
    {
        public readonly static string RequestParameterName = "_request_";
        public readonly static string ExternalRequestVariableName = "@req";
        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class {className} : DynamicObjectTextPartBase, IObjectTextPart
{
    public {className}(IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public dynamic? Get(RequestData " + RequestParameterName + @")
    {
        var " + ExternalRequestVariableName + @" = new RequestModel(" + RequestParameterName + @");

        {code}
    }
}";
        
        public readonly static string IGlobalObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class {className} : DynamicObjectTextPartBase, IGlobalObjectTextPart
{
    public {className}(IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public dynamic? Get(RequestData _request_) => GetInternal(_request_);

    public dynamic? Get() => GetInternal(null);

    public dynamic? GetInternal(RequestData? _request_)
    {
        {code}
    }
}";
    }
}

