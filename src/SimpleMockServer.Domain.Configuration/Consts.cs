namespace SimpleMockServer.Domain.Configuration;

class Consts
{
    public static class ExecutedBlock
    {
        public const string Begin = "{{";
        public const string End = "}}";

        public const char BeginChar = '{';
        public const char EndChar = '}';
    }
    
    public static class ControlChars
    {
        public const string Lambda = "=>";
        public const string HeaderSplitter = ":";
        public const string VariablePrefix = "$";
        public const string Comment = "//";
        public const string AssignmentOperator = "=";
    }
}
