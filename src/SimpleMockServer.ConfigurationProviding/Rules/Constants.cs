namespace SimpleMockServer.ConfigurationProviding.Rules;

class Constants
{
    public static class SectionName
    {
        public const string Rule = "rule";
        public const string Variables = "variables";
        public const string Condition = "condition";
        public const string Response = "response";
        public const string Callback = "callback";
    }
    
    public static class BlockName
    {
        public class Rule
        {
            public const string Method = "method";
            public const string Path = "path";
            public const string Query = "query";
            public const string Headers = "headers";
            public const string Body = "body";
        }

        public class Response
        {
            public const string Code = "code";
            public const string Headers = "headers";
            public const string Body = "body";
        }
    }
    
    public static readonly HashSet<string> HttpMethods = new()
    {
        "GET",
        "PUT",
        "POST",
        "DELETE",
        "HEAD",
        "TRACE",
        "OPTIONS",
        "CONNECT"
    };
    
    public static class ControlChars
    {
        public const string Lambda = "=>";
        public const string HeaderSplitter = ":";
        public const string VariablePrefix = "$";
    }
}
