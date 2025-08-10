namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

public class TokenParsingException : Exception
{
    public TokenParsingException(string message) : base(message) { }
}