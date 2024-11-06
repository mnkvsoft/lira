using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

static partial class ClassCodeCreator
{
    public static string CreateIObjectTextPart(
        string className,
        string code,
        string contextParameterName,
        string externalRequestVariableName,
        string repeatFunctionName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IObjectTextPart
            .Replace("[className]", className)
            .Replace("[code]", code)
            .Replace("[context]", contextParameterName)
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

    public static string CreateRequestMatcher(
        string className,
        string code,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IRequestMatcher
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[className]", className)
            .Replace("[code]", code);
    }

    public static string CreateAction(
        string className,
        string code,
        string contextParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return CodeTemplate.IAction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[context]", contextParameterName)
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
}


