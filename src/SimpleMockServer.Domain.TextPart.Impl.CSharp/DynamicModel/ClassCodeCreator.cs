namespace SimpleMockServer.Domain.TextPart.Impl.CSharp.DynamicModel;

static class ClassCodeCreator
{
    public static string CreateIObjectTextPart(
        string className,
        string code,
        string requestParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CreateClassCode(CodeTemplate.IObjectTextPart, className, code, requestParameterName, namespaces, usingStaticTypes)
                    .Replace("[externalRequestVariableName]", externalRequestVariableName);
    }

    public static string CreateITransformFunction(
        string className,
        string code,
        string inputArgumentName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.ITransformFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[className]", className)
            .Replace("[input]", inputArgumentName)
            .Replace("[code]", code);
    }

    private static string CreateClassCode(
        string template, 
        string className, 
        string code, 
        string requestParameterName, 
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return template
                    .Replace("[className]", className)
                    .Replace("[code]", code)
                    .Replace("[request]", requestParameterName)
                    .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
                    .Replace("[namespaces]", GetNamespaces(namespaces));
    }

    private static string GetNamespaces(IReadOnlyCollection<string> namespaces)
    {
        return namespaces.Count == 0 ? "" : string.Join(Environment.NewLine, namespaces.Select(n => $"using {n};"));
    }

    private static string GetUsingStatic(IReadOnlyCollection<string> usingStaticTypes)
    {
        return usingStaticTypes.Count == 0 ? "" : string.Join(Environment.NewLine, usingStaticTypes.Select(n => $"using static {n};"));
    }

    static class CodeTemplate
    {
        private static readonly string Nl = Environment.NewLine;

        private readonly static string ImportNamespaces =
            "using System;" + Nl +
            "using System.Text;" + Nl +
            "using System.Linq;" + Nl +
            "using System.Collections;" + Nl +
            "using System.Collections.Generic;" + Nl + Nl +

            "using SimpleMockServer.Domain.TextPart.Impl.CSharp.DynamicModel;" + Nl +
            "using SimpleMockServer.Domain;" + Nl +

            "using SimpleMockServer.Domain.TextPart;" + Nl +
            "using SimpleMockServer.Domain.TextPart.Impl.CSharp;" + Nl +
            "using static SimpleMockServer.Domain.TextPart.Impl.CSharp.Functions.JsonUtils;" + Nl +
            "[namespaces]";

        public const string Namespace = "namespace __DynamicGenerated;";

        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class [className] : DynamicObjectTextPartBase, IObjectTextPart
{
    public [className](IDeclaredPartsProvider declaredPartsProvider) : base(declaredPartsProvider)
    {
    }

    public dynamic? Get(RequestData [request])
    {
        var [externalRequestVariableName] = new RequestModel([request]);
        
        [code]
    }
}";

        public readonly static string ITransformFunction =
            "[namespaces]" + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
@"namespace SimpleMockServer.Domain.TextPart.Impl.CSharp.DynamicModel;

public class [className] : ITransformFunction
{
    public dynamic? Transform(dynamic? [input])
    {
        return [code];
    }
}
";
    }
}


