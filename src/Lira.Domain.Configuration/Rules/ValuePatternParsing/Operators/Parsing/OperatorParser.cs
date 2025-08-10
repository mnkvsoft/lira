namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class OperatorParser(IEnumerable<OperatorDefinition> definitions)
{
    private readonly TokenParser _tokenParser = new(definitions);

    public IReadOnlyList<Token> Parse(string text)
    {
        var parts = PatternParser.Parse(text);
        return _tokenParser.Parse(parts);
    }
}