namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers.ParametersParsing;

record ParamsParseResult
{
    public record Success(ISet<MethodParameter> Parameters) : ParamsParseResult
    {
        public override string ToString()
        {
            return "[" + string.Join(", ", Parameters) + "]";
        }
    }

    public record Fail(string Message) : ParamsParseResult
    {
        public override string ToString()
        {
            return $"[fail: {Message}]";
        }
    }
}