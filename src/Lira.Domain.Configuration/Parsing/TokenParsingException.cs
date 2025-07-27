namespace Lira.Domain.Configuration.Parsing;

public class TokenParsingException : Exception
{
    public TokenParsingException(string message) : base(message) { }
}