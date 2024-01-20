using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

static class ClassCodeCreator
{
    public static string CreateIObjectTextPart(
        string className,
        string code,
        string requestParameterName,
        string externalRequestVariableName,
        string repeatFunctionName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IObjectTextPart
            .Replace("[className]", className)
            .Replace("[code]", code)
            .Replace("[request]", requestParameterName)
            .Replace("[repeat]", repeatFunctionName)
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
    
    public static string CreateAction(
        string className,
        string code,
        string requestParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IAction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[request]", requestParameterName)
            .Replace("[className]", className)
            .Replace("[externalRequestVariableName]", externalRequestVariableName)
            .Replace("[code]", code);
    }

    private static string GetNamespaces(IReadOnlyCollection<string> namespaces)
    {
        return namespaces.Count == 0 ? "" : string.Join(Constants.NewLine, namespaces.Select(n => $"using {n};"));
    }

    private static string GetUsingStatic(IReadOnlyCollection<string> usingStaticTypes)
    {
        return usingStaticTypes.Count == 0 ? "" : string.Join(Constants.NewLine, usingStaticTypes.Select(n => $"using static {n};"));
    }

    static class CodeTemplate
    {
        private static readonly string Nl = Constants.NewLine;

        private readonly static string ImportNamespaces =
            "using System;" + Nl +
            "using System.IO;" + Nl +
            "using System.Text;" + Nl +
            "using System.Linq;" + Nl +
            "using System.Collections;" + Nl +
            "using System.Collections.Generic;" + Nl + Nl +
            "using System.Threading.Tasks;" + Nl + Nl +

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
public class [className] : DynamicObjectBaseGenerate, IObjectTextPart
{
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public dynamic? Get(RequestData [request])
    {
        var [externalRequestVariableName] = new RequestModel([request]);
        
        [code]

        string [repeat](IObjectTextPart part, string separator = "","", int? count = null, int? from = null, int? to = null)
        {
            int cnt;
            if(count != null)
                cnt = count.Value;
            else if(from != null)
                cnt = Random.Shared.Next(from.Value, to.Value + 1);
            else
                cnt = Random.Shared.Next(3, 9);
            return Repeat([request], part, separator, cnt);
        }
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
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public dynamic? Transform(dynamic? [input])
    {
        [code]
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

public class [className] : DynamicObjectBaseMatch, IMatchFunction
{
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
    public bool IsMatch(string? [input])
    {
        [code]
    }
}
";
        
        public readonly static string IAction =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Actions;" + Nl +
            @"

public class [className] : DynamicObjectBaseAction, IAction
{
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public async Task Execute(RequestData [request])
    {
        var [externalRequestVariableName] = new RequestModel([request]);
        [code]
    }

}
";
        
    }
}


