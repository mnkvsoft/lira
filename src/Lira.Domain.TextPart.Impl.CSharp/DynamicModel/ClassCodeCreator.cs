using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

class ClassWithoutName(string value)
{
    public string SetClassName(string name)
    {
        return value.Replace("[className]", name);
    }
}

static partial class ClassCodeCreator
{
    public static ClassWithoutName CreateIObjectTextPart(
        string returnType,
        string code,
        string contextParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> usings,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.IObjectTextPart
            .Replace("[type]", returnType)
            .Replace("[code]", code)
            .Replace("[context]", contextParameterName)
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usings]", GetUsings(usings))
            .Replace("[externalRequestVariableName]", externalRequestVariableName));
    }

    public static ClassWithoutName CreateTransformFunction(
        string code,
        string inputArgumentName,
        IReadOnlyCollection<string> usings,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.ITransformFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[usings]", GetUsings(usings))
            .Replace("[input]", inputArgumentName)
            .Replace("[code]", code));
    }

    public static ClassWithoutName CreateMatchFunction(
        string code,
        string inputArgumentName,
        IReadOnlyCollection<string> usings,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.IMatchFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[usings]", GetUsings(usings))
            .Replace("[input]", inputArgumentName)
            .Replace("[code]", code));
    }

    public static ClassWithoutName CreateRequestMatcher(
        string code,
        string externalRequestVariableName,
        IReadOnlyCollection<string> usings,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.IRequestMatcher
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[usings]", GetUsings(usings))
            .Replace("[externalRequestVariableName]", externalRequestVariableName)
            .Replace("[code]", code));
    }

    public static ClassWithoutName CreatePredicateFunction(
        string code,
        string externalRequestVariableName,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.IPredicateFunction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[usings]", "")
            .Replace("[externalRequestVariableName]", externalRequestVariableName)
            .Replace("[code]", code));
    }

    public static ClassWithoutName CreateAction(
        string code,
        string contextParameterName,
        string externalRequestVariableName,
        IReadOnlyCollection<string> usings,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> usingStaticTypes)
    {
        return new ClassWithoutName(CodeTemplate.IAction
            .Replace("[namespaces]", GetNamespaces(namespaces))
            .Replace("[usingstatic]", GetUsingStatic(usingStaticTypes))
            .Replace("[usings]", GetUsings(usings))
            .Replace("[context]", contextParameterName)
            .Replace("[externalRequestVariableName]", externalRequestVariableName)
            .Replace("[code]", code));
    }

    private static string GetNamespaces(IReadOnlyCollection<string> namespaces)
    {
        return namespaces.Count == 0 ? "" : string.Join(Constants.NewLine, namespaces.Select(n => $"using {n};"));
    }

    private static string GetUsingStatic(IReadOnlyCollection<string> usingStaticTypes)
    {
        return usingStaticTypes.Count == 0 ? "" : string.Join(Constants.NewLine, usingStaticTypes.Select(n => $"using static {n};"));
    }

    private static string GetUsings(IReadOnlyCollection<string> usings)
    {
        return usings.Count == 0 ? "" : string.Join(Constants.NewLine, usings.Select(x => x.EndsWith(';') ? x : x + ";"));
    }
}


