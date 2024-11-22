namespace Lira.Domain.Configuration;

class Consts
{
    public static class ExecutedBlock
    {
        public const char BeginChar = '{';
        public const char EndChar = '}';
    }

    public static class ControlChars
    {
        public const string PipelineSplitter = ">>";
        public const string WriteToVariablePrefix = ">>";
        public const string HeaderSplitter = ":";
        public const string SetType = ":";
        public const string VariablePrefix = "$$";
        public const string FunctionPrefix = "$";
        public const string TemplatePrefix = "@@";
        public const string AssignmentOperator = "=";
    }

    public static class Type
    {
        public const string Json = "json";
        public const string String = "str";
        public const string Int = "int";
        public const string Guid = "guid";
        public const string Decimal = "dec";
    }
}
