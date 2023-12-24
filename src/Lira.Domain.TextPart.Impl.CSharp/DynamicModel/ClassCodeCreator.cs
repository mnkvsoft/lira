namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

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
        return CodeTemplate.IObjectTextPart
            .Replace("[className]", className)
            .Replace("[code]", code)
            .Replace("[request]", requestParameterName)
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[namespaces]", GetNamespaces(namespaces)).Replace("[externalRequestVariableName]", externalRequestVariableName);
    }

    public static string CreateTransformFunction(
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
    
    public static string CreateMatchFunction(
        string className,
        string code,
        string inputArgumentName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IMatchFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[className]", className)
            .Replace("[input]", inputArgumentName)
            .Replace("[code]", code);
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

            "using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;" + Nl +
            "using Lira.Domain;" + Nl +

            "using Lira.Domain.TextPart;" + Nl +
            "using Lira.Domain.TextPart.Impl.CSharp;" + Nl +
            "using static Lira.Domain.TextPart.Impl.CSharp.Functions.JsonUtils;" + Nl +
            "[namespaces]";

        public const string Namespace = "namespace __DynamicGenerated;";

        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class [className] : DynamicObjectBase, IObjectTextPart
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
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public class [className] : DynamicObjectBase, ITransformFunction
{
    public [className](IDeclaredPartsProvider declaredPartsProvider) : base(declaredPartsProvider)
    {
    }

    public dynamic? Transform(dynamic? [input])
    {
        return [code];
    }
}
";
        
        public readonly static string IMatchFunction =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            @"

public class [className] : DynamicObjectBase, IMatchFunction
{
    public [className](IDeclaredPartsProvider declaredPartsProvider) : base(declaredPartsProvider)
    {
    }

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
    public bool IsMatch(string? [input])
    {
        return [code];
    }
}
";
    }
}


