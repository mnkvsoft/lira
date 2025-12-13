namespace Lira.Domain.Configuration.Rules;

class Constants
{
    public static class SectionName
    {
        public const string Rule = "rule";
        public const string Declare = "declare";
        public const string Config = "config";
        public const string Condition = "condition";
        public const string Response = "response";
        public const string ActionPrefix = "action";
        public const string Cache = "cache";
    }

    public static class BlockName
    {
        public class Common
        {
            public const string Delay = "delay";
        }

        public class Rule
        {
            public const string Method = "method";
            public const string Path = "path";
            public const string Query = "query";
            public const string Headers = "headers";
            public const string Body = "body";
            public const string Request = "req";
        }

        public class Response
        {
            public const string Code = "code";
            public const string Headers = "headers";
            public const string Body = "body";
            public const string Abort = "fault";
            public const string Delay = Common.Delay;
        }

        public class Action
        {
            public const string Code = "code";
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
}
