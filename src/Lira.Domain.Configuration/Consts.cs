namespace Lira.Domain.Configuration;

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
        public const string PipelineSplitter = ">>";
        public const string HeaderSplitter = ":";
        public const string VariablePrefix = "$$";
        public const char SystemVariablePrefix = '@';
        public const string FunctionPrefix = "$";
        public const string TemplatePrefix = "@@";
        public const string AssignmentOperator = "=";
    }
}
