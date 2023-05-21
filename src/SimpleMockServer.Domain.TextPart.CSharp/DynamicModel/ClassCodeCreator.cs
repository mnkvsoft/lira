namespace SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;

static class ClassCodeCreator
{
    public static string CreateIGlobalObjectTextPart(
        string className,
        string code,
        string requestParameterName,
        IReadOnlyCollection<string> namespaces)
    {
        return CreateClassCode(CodeTemplate.IGlobalObjectTextPart, className, code, requestParameterName, namespaces);
    }

    public static string CreateIObjectTextPart(
        string className,
        string code,
        string requestParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> namespaces)
    {
        return CreateClassCode(CodeTemplate.IObjectTextPart, className, code, requestParameterName, namespaces)
                    .Replace("[externalRequestVariableName]", externalRequestVariableName);
    }

    public static string CreateITransformFunction(
        string className,
        string code,
        string inputArgumentName,
        IReadOnlyCollection<string> namespaces)
    {
        return CodeTemplate.ITransformFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[className]", className)
            .Replace("[input]", inputArgumentName)
            .Replace("[code]", code);
    }

    private static string CreateClassCode(string template, string className, string code, string requestParameterName, IReadOnlyCollection<string> namespaces)
    {
        return template
                    .Replace("[className]", className)
                    .Replace("[code]", code)
                    .Replace("[request]", requestParameterName)
                    .Replace("[namespaces]", GetNamespaces(namespaces));
    }

    private static string GetNamespaces(IReadOnlyCollection<string> namespaces)
    {
        return namespaces.Count == 0 ? "" : string.Join(Environment.NewLine, namespaces.Select(n => $"using {n};"));
    }

    static class CodeTemplate
    {
        private static readonly string Nl = Environment.NewLine;

        private readonly static string ImportNamespaces =
            "using System;" + Nl +
            "using System.Text;" + Nl +
            "using System.Collections;" + Nl +
            "using System.Collections.Generic;" + Nl + Nl +

            "using SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;" + Nl +
            "using SimpleMockServer.Domain;" + Nl +

            "using SimpleMockServer.Domain.TextPart;" + Nl +
            "using SimpleMockServer.Domain.TextPart.Variables;" + Nl +
            "[namespaces]";

        public const string Namespace = "namespace __DynamicGenerated;";

        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class [className] : DynamicObjectTextPartBase, IObjectTextPart
{
    public [className](IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public dynamic? Get(RequestData [request])
    {
        var [externalRequestVariableName] = new RequestModel([request]);

        [code]
    }
}";

        public readonly static string ITransformFunction =
            "[namespaces]" + Nl +
@"namespace SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;

public class [className] : ITransformFunction
{
    public dynamic? Transform(dynamic? [input])
    {
        return [code];
    }
}
";

        public readonly static string IGlobalObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class [className] : DynamicObjectTextPartBase, IGlobalObjectTextPart
{
    public [className](IReadOnlyCollection<Variable> variables) : base(variables)
    {
    }

    public dynamic? Get(RequestData [request]) => GetInternal([request]);

    public dynamic? Get() => GetInternal(null);

    public dynamic? GetInternal(RequestData? [request])
    {
        [code]
    }
}";

    }
}


