namespace SimpleMockServer.Domain.TextPart.CSharp;

static class CodeTemplate
{
    public const string ImportNamespaces =
        @"
using System;
using System.Text;
using System.Collections;

using SimpleMockServer.Domain;
using SimpleMockServer.Domain.TextPart;";

    public const string Namespace = "namespace __DynamicGenerated;";
    
    public static class ClassTemplate
    {
        public const string IObjectTextPart =
            ImportNamespaces +
            Namespace +
            @"
public class {className} : IObjectTextPart
{
    public object Get(RequestData request)
    {
        return {code};
    }
}";
    }
}

